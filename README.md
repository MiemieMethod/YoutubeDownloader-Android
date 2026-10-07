# YoutubeDownloader for Android

[中文](#中文) | [English](#english)

## 中文

这是 [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) 的安卓移植版，用于从 YouTube 下载视频。下载核心基于 [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) 和 YoutubeExplode.Converter，界面使用 .NET MAUI，并内置了一个专为安卓编译的精简版 FFmpeg。

### 安装

- 直接安装仓库中的 [`dist/YoutubeDownloader-1.0.0.apk`](dist/YoutubeDownloader-1.0.0.apk)。这是通用 APK，支持 arm64-v8a、armeabi-v7a 和 x86_64 设备。也可以从 [Releases](https://github.com/MiemieMethod/YoutubeDownloader-Android/releases) 或 GitHub Actions 的构建产物中下载。
- 需要 Android 5.0（API 21）或更高版本。安装时需要允许“安装未知来源应用”。
- 不同来源的 APK 可能使用不同的签名密钥。换用其他来源的 APK 时，需要先卸载旧版本。

### 功能

与桌面版 YoutubeDownloader 相同的功能：

- 可以输入视频链接或 ID、播放列表链接或 ID、频道链接（包括 `@handle`、自定义链接和旧版用户链接），或者搜索关键词。在内容前加 `?` 可以强制按关键词搜索。一次可以输入多行，结果会合并显示。
- 单个视频：列出所有可用的格式和清晰度（mp4、webm、mp3、ogg），并可以修改文件名。
- 多个视频（播放列表、频道、搜索结果）：可以多选或全选，并统一选择容器格式和清晰度偏好。
- 下载时可以加入其他语言的音轨、字幕，以及元数据标签（标题、作者、封面等）。
- 可以自定义文件名模板，支持 `$num`、`$numc`、`$id`、`$title`、`$author`、`$uploadDate`。
- 可以限制同时下载的数量，也可以跳过已经存在的文件。
- 下载列表中的任务可以取消、重试和查看错误信息。也可以批量操作：清除已完成或已结束的任务、重试失败的任务、取消全部任务。
- 可以登录 Google 账号，用于下载有年龄限制、私享或会员专属的视频。登录状态可以选择加密保存。
- 支持跟随系统、浅色和深色三种主题，以及 7 种界面语言：英语、简体中文、乌克兰语、德语、法语、西班牙语、匈牙利语。
- 通过 GitHub Releases 检查更新。

安卓版特有的功能：

- 在 YouTube 应用或浏览器中点击“分享”并选择本应用，即可直接解析链接。
- 下载在前台服务中进行，通知栏会显示进度。切换到后台后下载也会继续。
- 文件默认保存到 `Download/YoutubeDownloader`，也可以在设置中改为任意其他文件夹。
- 下载完成后可以直接打开或分享文件。

与桌面版的差异：

- FFmpeg 已经内置在应用中，所以设置里没有 FFmpeg 路径选项。
- 安卓不允许应用在后台静默更新自己，所以“自动更新”只会提示有新版本，并提供下载链接。
- 桌面版的“另存为”对话框改成了文件名输入框，保存位置在设置中的“下载文件夹”里选择。

### 从源码构建

需要准备：

- Linux 或 WSL，并安装 `curl`、`make`、`nasm`
- [.NET SDK 10](https://dotnet.microsoft.com/download)，并安装安卓工作负载：`dotnet workload install maui-android`
- Android SDK（API 36）、Android NDK r27 或更新版本，以及 JDK 17

步骤：

```sh
# 1. 为安卓交叉编译 FFmpeg。脚本会下载源码，结果输出到 ffmpeg/out/<abi>/libffmpeg.so
export ANDROID_NDK_HOME=/path/to/android-ndk
ffmpeg/build.sh

# 2. 构建 APK
dotnet publish YoutubeDownloader/YoutubeDownloader.csproj -f net10.0-android -c Release -o publish
```

生成的 APK 是 `publish/io.github.miemiemethod.youtubedownloader-Signed.apk`。如果要用自己的密钥签名，可以加上这些参数：`-p:AndroidKeyStore=true -p:AndroidSigningKeyStore=<keystore 路径> -p:AndroidSigningKeyAlias=<别名> -p:AndroidSigningStorePass=env:<环境变量名> -p:AndroidSigningKeyPass=env:<环境变量名>`。

GitHub Actions（`.github/workflows/main.yml`）会在每次推送时自动构建 APK，推送 `v*` 标签时还会自动创建 Release。如果在仓库中配置了 `ANDROID_KEYSTORE_BASE64`、`ANDROID_KEYSTORE_PASSWORD`、`ANDROID_KEY_ALIAS` 和 `ANDROID_KEY_PASSWORD` 这几个 Secrets，就会用你的密钥签名；否则使用自动生成的调试密钥。

### 项目结构

- `YoutubeDownloader.Core/`：下载核心，从上游移植，只为安卓做了少量调整（Cookie 处理、文件名规则、FFmpeg 路径）。
- `YoutubeDownloader/`：.NET MAUI 安卓应用，包括视图模型、页面和本地化，以及存储、通知、分享链接等安卓平台代码。
- `ffmpeg/`：FFmpeg 的安卓构建脚本和补丁。

## English

This is an Android port of [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader). It is built with .NET MAUI on top of [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) and YoutubeExplode.Converter. A minimal FFmpeg build for Android is bundled with the app.

- **Install**: use [`dist/YoutubeDownloader-1.0.0.apk`](dist/YoutubeDownloader-1.0.0.apk), or download the APK from Releases or the GitHub Actions artifacts. It is a universal APK for arm64-v8a, armeabi-v7a and x86_64 devices and needs Android 5.0 or later.
- **Features**: the same as the desktop app.
  - Download videos, playlists, channels and search results.
  - Choose the format and quality.
  - Add subtitles, audio tracks in other languages, and tags.
  - Use file name templates, limit parallel downloads, and skip existing files.
  - Sign in with a Google account.
  - Choose a theme and language, and check for updates.
- **Android extras**:
  - Share links to the app from other apps.
  - Downloads keep running in the background, with a progress notification.
  - Choose a custom download folder.
  - Open or share downloaded files.
- **Build**: run `ffmpeg/build.sh`, which needs the Android NDK and `nasm`. Then run `dotnet publish YoutubeDownloader/YoutubeDownloader.csproj -f net10.0-android -c Release`.

## License

- The app is derived from [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) by Oleksii Holub and is distributed under the MIT license (see [`License.txt`](License.txt)). Please also review the upstream project's terms of use.
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) is licensed under the MIT license.
- The bundled FFmpeg is licensed under LGPL 2.1 or later. It is built without GPL or non-free components, from the sources and patch referenced in [`ffmpeg/build.sh`](ffmpeg/build.sh) and [`ffmpeg/patches`](ffmpeg/patches).
- LAME is licensed under the LGPL. libogg, libvorbis and Opus are licensed under BSD licenses.
- [Material Design Icons](https://pictogrammers.com/library/mdi/) are licensed under the Apache License 2.0.
