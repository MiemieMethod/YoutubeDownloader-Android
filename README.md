# YoutubeDownloader for Android

[中文](#中文) | [English](#english)

## 中文

这是 [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) 的安卓移植版，用于从 YouTube 下载视频。下载核心基于 [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) 和 YoutubeExplode.Converter，界面使用 .NET MAUI，并内置了一个专为安卓编译的精简版 FFmpeg。

### 安装

- 从 [Releases](https://github.com/MiemieMethod/YoutubeDownloader-Android/releases) 下载 `YoutubeDownloader.apk`，或者从 GitHub Actions 中最近一次成功运行的构建产物 `YoutubeDownloader` 中下载（需要登录 GitHub，下载得到的是 zip，解压后就是 APK）。这是通用 APK，支持 arm64-v8a、armeabi-v7a 和 x86_64 设备。
- 需要 Android 5.0（API 21）或更高版本。安装时需要允许“安装未知来源应用”。
- 从 1.0.3 开始，APK 由 GitHub Actions 使用仓库 Secrets 中的固定密钥签名，以后的版本可以直接覆盖安装。1.0.2 及更早的版本用的是其他密钥，需要先卸载旧版本再安装（卸载后需要重新登录和设置）。

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
- 通过 GitHub Releases 检查更新（见下文“发布新版本”）。

安卓版特有的功能：

- 在 YouTube 应用或浏览器中点击“分享”并选择本应用，即可直接解析链接。
- 下载在前台服务中进行，通知栏会显示进度。切换到后台后下载也会继续。
- 文件默认保存到 `Download/YoutubeDownloader`，也可以在设置中改为任意其他文件夹。
- 下载完成后可以直接打开或分享文件。

### 关于登录

YouTube 经常要求登录后才能获取视频（提示“确认你不是机器人”），使用 VPN、代理或共享网络时尤其常见。如果出现这类错误，请点击主界面顶部的身份验证按钮登录 Google 账号，然后重试。

目前 YoutubeExplode 6.6.2 在带登录信息时会被 YouTube 拒绝，报错 `400 Bad Request`（上游问题 [Tyrrrz/YoutubeExplode#969](https://github.com/Tyrrrz/YoutubeExplode/issues/969)）。本应用参照 yt-dlp 的做法绕过了这个问题：登录后改用支持 Cookie 的 TV 客户端获取视频流，并在隐藏的 WebView 中运行 yt-dlp 的 [EJS](https://github.com/yt-dlp/ejs) 脚本，解出 YouTube 播放器的签名和 `n` 参数。如果 TV 客户端不可用，会退回到不带登录信息的原始客户端。相关代码在 `YoutubeDownloader.Core/Youtube/` 中。

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

### 签名密钥

GitHub Actions（`.github/workflows/main.yml`）会在每次推送时构建 APK，并用以下 4 个仓库 Secrets 中的密钥签名：

| Secret | 内容 |
| --- | --- |
| `ANDROID_KEYSTORE_BASE64` | keystore 文件的 Base64 编码 |
| `ANDROID_KEYSTORE_PASSWORD` | keystore 的密码 |
| `ANDROID_KEY_ALIAS` | 密钥别名 |
| `ANDROID_KEY_PASSWORD` | 密钥的密码（PKCS12 格式的 keystore 中与 keystore 密码相同） |

不需要自己生成密钥，让工作流生成即可：

1. 在仓库还是**私有**的时候运行一次工作流。推送代码会自动触发；如果运行显示需要批准，打开它并点击 “Approve and run”。
2. 如果还没有配置这些 Secrets，工作流会生成一个新密钥，用它为本次构建的 APK 签名，并把 4 个 Secret 的值放进名为 `signing-secrets` 的构建产物（只保留 1 天）。
3. 下载并解压 `signing-secrets`，打开 Settings → Secrets and variables → Actions，点击 “New repository secret” 添加 4 个 Secret：名称是文件名（不含 `.txt`），值是文件的全部内容。具体说明见其中的 `README.txt`。
4. 把这些文件私下备份好，然后删除这个构建产物（或整个运行记录）。丢失密钥后，新版本将无法覆盖安装旧版本；泄露密钥后，别人可以冒充你发布“更新”。

之后所有的构建和 Release 都会使用这个密钥签名。另外：

- 公开仓库的构建产物任何人都能下载，所以只有私有仓库才会生成密钥。没有配置 Secrets 的公开仓库和 Pull Request 构建会使用临时的调试密钥签名。
- 发布 Release（推送 `v*` 标签）时必须已经配置好 Secrets，否则构建会失败，以免发布用临时密钥签名的版本。
- 也可以用 JDK 自带的 `keytool` 自己生成密钥：`keytool -genkeypair -keystore release.keystore -storetype PKCS12 -alias youtubedownloader -keyalg RSA -keysize 4096 -validity 10000`，再用 `base64 -w 0 release.keystore` 得到 `ANDROID_KEYSTORE_BASE64` 的值。

### 发布新版本

1. 在仓库页面打开 Releases → “Draft a new release”。
2. 在 “Choose a tag” 中输入一个新标签，例如 `v1.0.4`（格式为 `v主版本.次版本.修订号`，次版本号和修订号小于 100），Target 选择包含工作流文件的分支，然后点击 “Publish release”。
3. 工作流会用标签中的版本号构建 APK（不需要修改 `Directory.Build.props`），并把 `YoutubeDownloader.apk` 上传到这个 Release。直接推送标签也可以，工作流会自动创建 Release。

应用内的检查更新会访问 `https://github.com/MiemieMethod/YoutubeDownloader-Android/releases/latest`（不使用有频率限制的 GitHub API），所以只有在仓库公开、并且已经发布了 Release 时才能发现新版本。仓库为私有或还没有 Release 时，检查更新不会有任何提示。

### 支持本项目

本应用免费且开源。如果觉得好用，欢迎：

- 给[本仓库](https://github.com/MiemieMethod/YoutubeDownloader-Android)点个 Star，或者推荐给朋友；
- 在 [Issues](https://github.com/MiemieMethod/YoutubeDownloader-Android/issues) 中反馈问题、提出建议；
- 提交 Pull Request，一起来完善本应用。

### 项目结构

- `YoutubeDownloader.Core/`：下载核心，从上游移植，只为安卓做了少量调整（Cookie 处理、文件名规则、FFmpeg 路径）。`Youtube/` 目录中是登录状态下获取视频流的处理（TV 客户端和 EJS 挑战求解）。
- `YoutubeDownloader/`：.NET MAUI 安卓应用，包括视图模型、页面和本地化，以及存储、通知、分享链接等安卓平台代码。
- `ffmpeg/`：FFmpeg 的安卓构建脚本和补丁。

## English

This is an Android port of [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader). It is built with .NET MAUI on top of [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) and YoutubeExplode.Converter. A minimal FFmpeg build for Android is bundled with the app.

- **Install**: download `YoutubeDownloader.apk` from [Releases](https://github.com/MiemieMethod/YoutubeDownloader-Android/releases), or the `YoutubeDownloader` artifact of the latest successful GitHub Actions run. It is a universal APK for arm64-v8a, armeabi-v7a and x86_64 devices and needs Android 5.0 or later. Starting with 1.0.3, APKs are signed by GitHub Actions with a fixed key stored in the repository secrets, so later versions install over it. Versions 1.0.2 and earlier were signed with other keys, so uninstall them first.
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
- **Signing in**: YouTube often refuses to serve videos without signing in ("confirm you're not a bot"), especially over VPNs or shared networks. Sign in with the authentication button at the top of the main screen. YoutubeExplode 6.6.2 fails with `400 Bad Request` when cookies are used ([Tyrrrz/YoutubeExplode#969](https://github.com/Tyrrrz/YoutubeExplode/issues/969)). To work around this, the app follows yt-dlp's approach: signed-in requests use the cookie-capable TV client, and the player's signature and `n` challenges are solved by yt-dlp's [EJS](https://github.com/yt-dlp/ejs) scripts running in a hidden WebView. See `YoutubeDownloader.Core/Youtube/`.
- **Build**: run `ffmpeg/build.sh`, which needs the Android NDK and `nasm`. Then run `dotnet publish YoutubeDownloader/YoutubeDownloader.csproj -f net10.0-android -c Release`.
- **Signing**: GitHub Actions signs the APK with the key in the `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASSWORD`, `ANDROID_KEY_ALIAS` and `ANDROID_KEY_PASSWORD` secrets. If they are not configured, builds of a private repository generate a new key, sign the APK with it, and upload the 4 secret values as the `signing-secrets` artifact (kept for 1 day). Add them as repository secrets (name = file name without `.txt`, value = file content), back them up privately, and delete the artifact. Public repositories and pull requests fall back to a temporary debug key, and release builds fail without the secrets.
- **Releases**: create a release with a new tag such as `v1.0.4` on GitHub (or push the tag). The workflow builds the APK with the tag's version and attaches `YoutubeDownloader.apk` to the release. The in-app update check reads `https://github.com/MiemieMethod/YoutubeDownloader-Android/releases/latest`, so it only finds releases while the repository is public.
- **Support**: the app is free and open source. If you like it, star [the repository](https://github.com/MiemieMethod/YoutubeDownloader-Android) and share it, report bugs and ideas in [Issues](https://github.com/MiemieMethod/YoutubeDownloader-Android/issues), or send a pull request to help improve it.

## License

- The app is derived from [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) by Oleksii Holub and is distributed under the MIT license (see [`License.txt`](License.txt)).
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) is licensed under the MIT license.
- The bundled FFmpeg is licensed under LGPL 2.1 or later. It is built without GPL or non-free components, from the sources and patch referenced in [`ffmpeg/build.sh`](ffmpeg/build.sh) and [`ffmpeg/patches`](ffmpeg/patches).
- LAME is licensed under the LGPL. libogg, libvorbis and Opus are licensed under BSD licenses.
- The JavaScript challenge solver from [yt-dlp/ejs](https://github.com/yt-dlp/ejs) (release 0.8.0, `YoutubeDownloader.Core/Youtube/Ejs/`) is released under the Unlicense. It bundles [meriyah](https://github.com/meriyah/meriyah) (ISC license) and [astring](https://github.com/davidbonnet/astring) (MIT license). Their license texts are kept in the file headers.
- [Material Design Icons](https://pictogrammers.com/library/mdi/) are licensed under the Apache License 2.0.
