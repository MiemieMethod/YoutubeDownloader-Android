#!/usr/bin/env bash
#
# Cross-compiles a minimal FFmpeg command-line executable for Android.
#
# The resulting executable is placed at `ffmpeg/out/<abi>/libffmpeg.so`. It is
# named like a shared library so that Android extracts it into the app's native
# library directory, which is the only location an app is allowed to execute
# binaries from (W^X policy on Android 10+).
#
# The build only includes the components required by YoutubeExplode.Converter:
# - demuxers for the streams served by YouTube (mp4/webm) and subtitles (srt)
# - muxers for the supported output containers (mp4, webm, mp3, ogg)
# - audio encoders used when transcoding (aac, mp3 via LAME, vorbis, opus)
# - subtitle encoders (mov_text for mp4, webvtt for webm)
# Video streams are always copied as-is, so no video encoders are needed.
#
# All included components are licensed under LGPL (FFmpeg, LAME) or BSD (Ogg,
# Vorbis, Opus) licenses.
#
# Usage: ffmpeg/build.sh [abi...]
#   abi: arm64-v8a, armeabi-v7a, x86_64, x86 (default: arm64-v8a armeabi-v7a x86_64)
#
# Environment variables:
#   ANDROID_NDK_HOME  path to the Android NDK (r27 or newer recommended)
#   API_LEVEL         minimum Android API level (default: 21)
#   JOBS              number of parallel build jobs (default: number of CPUs)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WORK_DIR="${WORK_DIR:-$SCRIPT_DIR/.build}"
OUT_DIR="${OUT_DIR:-$SCRIPT_DIR/out}"
API_LEVEL="${API_LEVEL:-21}"
JOBS="${JOBS:-$(nproc 2>/dev/null || echo 2)}"

if [ "$#" -gt 0 ]; then
  ABIS=("$@")
else
  ABIS=(arm64-v8a armeabi-v7a x86_64)
fi

# Sources (name|sha256|url...)
FFMPEG_VERSION="8.1.2"
SOURCES=(
  "ffmpeg-$FFMPEG_VERSION.tar.xz|464beb5e7bf0c311e68b45ae2f04e9cc2af88851abb4082231742a74d97b524c|https://ffmpeg.org/releases/ffmpeg-$FFMPEG_VERSION.tar.xz|https://deb.debian.org/debian/pool/main/f/ffmpeg/ffmpeg_$FFMPEG_VERSION.orig.tar.xz|https://snapshot.debian.org/file/e918a340016f3f0633daa0b6a95e4ce3c58062ee"
  "lame-3.100.tar.gz|ddfe36cab873794038ae2c1210557ad34857a4b6bdc515785d1da9e175b1da1e|https://downloads.sourceforge.net/project/lame/lame/3.100/lame-3.100.tar.gz|https://deb.debian.org/debian/pool/main/l/lame/lame_3.100.orig.tar.gz|https://snapshot.debian.org/file/64c53b1a4d493237cef5e74944912cd9f98e618d"
  "libogg-1.3.6.tar.xz|5c8253428e181840cd20d41f3ca16557a9cc04bad4a3d04cce84808677fa1061|https://github.com/xiph/ogg/releases/download/v1.3.6/libogg-1.3.6.tar.xz|https://downloads.xiph.org/releases/ogg/libogg-1.3.6.tar.xz|https://deb.debian.org/debian/pool/main/libo/libogg/libogg_1.3.6.orig.tar.xz"
  "libvorbis-1.3.7.tar.gz|0e982409a9c3fc82ee06e08205b1355e5c6aa4c36bca58146ef399621b0ce5ab|https://github.com/xiph/vorbis/releases/download/v1.3.7/libvorbis-1.3.7.tar.gz|https://downloads.xiph.org/releases/vorbis/libvorbis-1.3.7.tar.gz|https://deb.debian.org/debian/pool/main/libv/libvorbis/libvorbis_1.3.7.orig.tar.gz"
  "opus-1.6.1.tar.xz|001ac615b9f8f8994b229ad2b1a5969ed12ab48b0d154d0af1016a5899cd6ba1|https://deb.debian.org/debian/pool/main/o/opus/opus_1.6.1.orig.tar.xz|https://snapshot.debian.org/file/5a6984ce44e568eaae11359f21e70d230d31bba9"
)

log() {
  echo "==> $*" >&2
}

die() {
  echo "error: $*" >&2
  exit 1
}

find_ndk() {
  local ndk="${ANDROID_NDK_HOME:-${ANDROID_NDK_ROOT:-${ANDROID_NDK:-}}}"
  if [ -z "$ndk" ]; then
    local sdk="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-}}"
    if [ -n "$sdk" ] && [ -d "$sdk/ndk" ]; then
      ndk="$(find "$sdk/ndk" -mindepth 1 -maxdepth 1 -type d | sort -V | tail -n 1)"
    fi
  fi
  [ -n "$ndk" ] && [ -d "$ndk" ] || die "Android NDK not found, set ANDROID_NDK_HOME"
  echo "$ndk"
}

download() {
  local spec="$1"
  local name sha urls
  IFS='|' read -r name sha urls <<<"$spec"
  local file="$WORK_DIR/sources/$name"

  if [ -f "$file" ] && echo "$sha  $file" | sha256sum -c --status; then
    echo "$file"
    return
  fi

  mkdir -p "$WORK_DIR/sources"
  local url
  IFS='|' read -r -a url_list <<<"$urls"
  for url in "${url_list[@]}"; do
    log "Downloading $name from $url"
    if curl -fsSL --retry 3 --connect-timeout 30 -o "$file.tmp" "$url" &&
      echo "$sha  $file.tmp" | sha256sum -c --status; then
      mv "$file.tmp" "$file"
      echo "$file"
      return
    fi
    log "Failed to download $name from $url"
    rm -f "$file.tmp"
  done

  die "could not download $name"
}

extract() {
  local archive="$1"
  local dest="$2"
  rm -rf "$dest"
  mkdir -p "$dest"
  tar -xf "$archive" -C "$dest" --strip-components=1
}

NDK="$(find_ndk)"
TOOLCHAIN="$NDK/toolchains/llvm/prebuilt/linux-x86_64"
[ -d "$TOOLCHAIN" ] || die "NDK toolchain not found at $TOOLCHAIN"
log "Using NDK at $NDK"

for spec in "${SOURCES[@]}"; do
  download "$spec" >/dev/null
done

build_abi() {
  local abi="$1"
  local triple host ffarch ffcpu ffflags=() page_size_flags=""

  case "$abi" in
  arm64-v8a)
    triple="aarch64-linux-android"
    host="aarch64-linux-android"
    ffarch="aarch64"
    ffcpu="armv8-a"
    ffflags=(--enable-neon)
    page_size_flags="-Wl,-z,max-page-size=16384"
    ;;
  armeabi-v7a)
    triple="armv7a-linux-androideabi"
    host="arm-linux-androideabi"
    ffarch="arm"
    ffcpu="armv7-a"
    ffflags=(--enable-neon --enable-thumb)
    ;;
  x86_64)
    triple="x86_64-linux-android"
    host="x86_64-linux-android"
    ffarch="x86_64"
    ffcpu="x86-64"
    page_size_flags="-Wl,-z,max-page-size=16384"
    ;;
  x86)
    triple="i686-linux-android"
    host="i686-linux-android"
    ffarch="x86"
    ffcpu="i686"
    # x86 assembly in FFmpeg relies on text relocations, which Android forbids
    ffflags=(--disable-asm)
    ;;
  *)
    die "unsupported ABI: $abi"
    ;;
  esac

  if [ "$ffarch" = "x86_64" ]; then
    if command -v nasm >/dev/null 2>&1; then
      ffflags+=(--x86asmexe=nasm)
    else
      log "nasm not found, building x86_64 without assembly optimizations"
      ffflags+=(--disable-x86asm)
    fi
  fi

  local build_dir="$WORK_DIR/$abi"
  local prefix="$build_dir/prefix"
  rm -rf "$build_dir"
  mkdir -p "$prefix"

  export CC="$TOOLCHAIN/bin/$triple$API_LEVEL-clang"
  export CXX="$TOOLCHAIN/bin/$triple$API_LEVEL-clang++"
  export AR="$TOOLCHAIN/bin/llvm-ar"
  export NM="$TOOLCHAIN/bin/llvm-nm"
  export RANLIB="$TOOLCHAIN/bin/llvm-ranlib"
  export STRIP="$TOOLCHAIN/bin/llvm-strip"
  export CFLAGS="-O2 -fPIC"
  export LDFLAGS="$page_size_flags"

  local cmake_args=(
    -G Ninja
    -DCMAKE_TOOLCHAIN_FILE="$NDK/build/cmake/android.toolchain.cmake"
    -DANDROID_ABI="$abi"
    -DANDROID_PLATFORM="android-$API_LEVEL"
    -DCMAKE_BUILD_TYPE=Release
    -DCMAKE_INSTALL_PREFIX="$prefix"
    -DCMAKE_INSTALL_LIBDIR=lib
    -DCMAKE_POSITION_INDEPENDENT_CODE=ON
    -DBUILD_SHARED_LIBS=OFF
    -DBUILD_TESTING=OFF
  )

  # libogg
  log "[$abi] Building libogg"
  extract "$WORK_DIR/sources/libogg-1.3.6.tar.xz" "$build_dir/libogg"
  cmake -S "$build_dir/libogg" -B "$build_dir/libogg/build" "${cmake_args[@]}" \
    -DINSTALL_DOCS=OFF >/dev/null
  cmake --build "$build_dir/libogg/build" --parallel "$JOBS" >/dev/null
  cmake --install "$build_dir/libogg/build" >/dev/null

  # libvorbis
  log "[$abi] Building libvorbis"
  extract "$WORK_DIR/sources/libvorbis-1.3.7.tar.gz" "$build_dir/libvorbis"
  cmake -S "$build_dir/libvorbis" -B "$build_dir/libvorbis/build" "${cmake_args[@]}" \
    -DOGG_INCLUDE_DIR="$prefix/include" \
    -DOGG_LIBRARY="$prefix/lib/libogg.a" >/dev/null
  cmake --build "$build_dir/libvorbis/build" --parallel "$JOBS" >/dev/null
  cmake --install "$build_dir/libvorbis/build" >/dev/null

  # opus
  log "[$abi] Building opus"
  extract "$WORK_DIR/sources/opus-1.6.1.tar.xz" "$build_dir/opus"
  cmake -S "$build_dir/opus" -B "$build_dir/opus/build" "${cmake_args[@]}" \
    -DOPUS_BUILD_PROGRAMS=OFF \
    -DOPUS_BUILD_TESTING=OFF \
    -DOPUS_INSTALL_PKG_CONFIG_MODULE=ON \
    -DOPUS_INSTALL_CMAKE_CONFIG_MODULE=OFF >/dev/null
  cmake --build "$build_dir/opus/build" --parallel "$JOBS" >/dev/null
  cmake --install "$build_dir/opus/build" >/dev/null

  # lame
  log "[$abi] Building lame"
  extract "$WORK_DIR/sources/lame-3.100.tar.gz" "$build_dir/lame"
  (
    cd "$build_dir/lame"
    ./configure \
      --host="$host" \
      --prefix="$prefix" \
      --enable-static \
      --disable-shared \
      --with-pic \
      --disable-frontend \
      --disable-decoder \
      --disable-gtktest \
      --disable-analyzer-hooks >/dev/null
    make -j"$JOBS" >/dev/null
    make install >/dev/null
  )

  # ffmpeg
  log "[$abi] Building FFmpeg $FFMPEG_VERSION"
  extract "$WORK_DIR/sources/ffmpeg-$FFMPEG_VERSION.tar.xz" "$build_dir/ffmpeg"
  local patch
  for patch in "$SCRIPT_DIR"/patches/*.patch; do
    patch -d "$build_dir/ffmpeg" -p1 --quiet <"$patch"
  done
  (
    cd "$build_dir/ffmpeg"
    PKG_CONFIG_LIBDIR="$prefix/lib/pkgconfig" PKG_CONFIG_PATH="" \
      ./configure \
      --prefix="$prefix" \
      --target-os=android \
      --arch="$ffarch" \
      --cpu="$ffcpu" \
      --enable-cross-compile \
      --cc="$CC" \
      --cxx="$CXX" \
      --ar="$AR" \
      --nm="$NM" \
      --ranlib="$RANLIB" \
      --strip="$STRIP" \
      --sysroot="$TOOLCHAIN/sysroot" \
      --pkg-config=pkg-config \
      --pkg-config-flags=--static \
      --extra-cflags="-I$prefix/include -ffunction-sections -fdata-sections" \
      --extra-ldflags="-L$prefix/lib -Wl,--gc-sections $page_size_flags" \
      --extra-libs="-lm" \
      "${ffflags[@]}" \
      --enable-pic \
      --enable-static \
      --disable-shared \
      --disable-debug \
      --disable-doc \
      --enable-small \
      --disable-autodetect \
      --disable-network \
      --disable-everything \
      --disable-programs \
      --enable-ffmpeg \
      --disable-avdevice \
      --disable-swscale \
      --enable-pthreads \
      --enable-zlib \
      --enable-libmp3lame \
      --enable-libvorbis \
      --enable-libopus \
      --enable-protocol=file,pipe \
      --enable-demuxer=mov,matroska,ogg,mp3,aac,srt,webvtt \
      --enable-muxer=mp4,mov,ipod,webm,matroska,mp3,ogg,oga,opus,adts,srt,webvtt,null \
      --enable-decoder=aac,opus,vorbis,mp3float,subrip,srt,webvtt,movtext,h264,vp9 \
      --enable-encoder=aac,libmp3lame,libvorbis,libopus,movtext,webvtt,subrip,srt \
      --enable-parser=aac,opus,vorbis,mpegaudio,h264,vp9,av1 \
      --enable-bsf=aac_adtstoasc,vp9_superframe,vp9_superframe_split,null \
      --enable-filter=aresample,aformat,anull,atrim,asetpts,apad,acopy,null,format,trim,setpts,copy >/dev/null
    make -j"$JOBS" ffmpeg >/dev/null
  )

  mkdir -p "$OUT_DIR/$abi"
  cp "$build_dir/ffmpeg/ffmpeg" "$OUT_DIR/$abi/libffmpeg.so"
  "$STRIP" --strip-all "$OUT_DIR/$abi/libffmpeg.so"
  chmod 755 "$OUT_DIR/$abi/libffmpeg.so"

  log "[$abi] Done: $OUT_DIR/$abi/libffmpeg.so ($(du -h "$OUT_DIR/$abi/libffmpeg.so" | cut -f1))"
}

for abi in "${ABIS[@]}"; do
  build_abi "$abi"
done
