# 食物图鉴 (Food Encyclopedia)

基于 .NET MAUI 的跨平台移动应用，用于浏览、搜索和分享世界各地美食。

## 功能

- **食物图鉴浏览**：查看食物名称、地区、描述和上传者信息
- **搜索**：按名称、地区、描述、标签搜索食物
- **用户系统**：用户名密码注册和登录（含10个测试用户，密码均为"123"）
- **上传食物**：添加新的食物条目，包含名称、地区、描述和拍照/相册图片
- **地图选地区**：通过原生地图（Google Maps / Apple Maps）选择食物地区，支持手动输入
- **摇一摇**：摇动设备弹出随机食物推荐
- **下拉刷新**：下拉重新随机排列所有食物卡片
- **上滑加载更多**：滚动到底部自动加载下一页（每页16条），已加载卡片保留
- **平板适配**：根据屏幕宽度自动切换1/2/3/4列网格布局
- **长按搜索**：长按食物卡片弹出对话框，可跳转浏览器搜索该食物
- **语音播放**：在详情页点击播放按钮，TTS 朗读食物描述
- **主题切换**：支持浅色/深色/跟随系统主题
- **大字体模式**：无障碍字体放大（1.22倍）

## 技术架构

- **框架**：.NET MAUI (net9.0)
- **数据来源**：支持从 mockapi.io REST API 获取数据，网络不可用时自动使用本地兜底数据
- **测试数据**：50条食物数据 + 10个测试用户预置在 JSON 资源文件中
- **本地存储**：用户数据和食物数据通过 JSON 文件持久化

## 项目结构

```
foodApp/
  Behaviors/
    LongPressBehavior.cs   # 跨平台长按行为组件
  Models/
    FoodItem.cs            # 食物数据模型
    User.cs                # 用户模型
  Services/
    MockApiConfig.cs       # mockapi.io 配置
    FoodService.cs         # 食物数据服务（API + JSON资源 + 本地兜底）
    UserService.cs         # 用户认证服务（JSON资源 + 本地持久化）
    AccessibilityService.cs # 无障碍字体缩放
    SpeechService.cs       # 文字转语音服务（TTS）
    MapConfig.cs           # Google Maps API Key 配置
  Pages/
    LoginPage.xaml         # 登录页
    RegisterPage.xaml      # 注册页
    MainPage.xaml          # 主页（食物卡片网格 + 分页）
    AddFoodPage.xaml       # 添加食物
    FoodDetailPage.xaml    # 食物详情（含语音播放按钮）
    MapPage.xaml           # 地图选址
    SettingsPage.xaml      # 设置页（主题 + 大字体）
  Platforms/Android/       # Android 平台配置
  Resources/
    Raw/
      test_food_data.json  # 50条测试食物数据
      test_users.json      # 10个测试用户数据
    Images/                # 食物SVG插图
    Styles/                # 全局样式
    Fonts/                 # 字体文件
```

## 构建与运行

### 前置要求

- .NET 9.0 SDK
- Visual Studio 2022（含 .NET MAUI 工作负载）

### Windows 构建

```powershell
dotnet build foodApp\foodApp.csproj -f net9.0-windows10.0.19041.0
```

### Android 构建

```powershell
dotnet build foodApp\foodApp.csproj -f net9.0-android
```

## MockAPI 配置

参见项目根目录下的 `mockapi配置说明.md`。

## 演示建议

1. 启动应用，展示登录/注册页面（可用"chef_li"/"123"等测试账户直接登录）
2. 浏览食物图鉴网格，观察卡片随机排列
3. 下拉刷新 → 卡片重新随机排列
4. 向下滚动 → 自动加载更多卡片
5. **长按**某张食物卡片 → 震动 → 弹出搜索确认 → 跳转浏览器
6. 搜索特定食物
7. 点击 Details → 查看详情 → 点击 🔊 Play 按钮听语音朗读
8. 添加新的食物条目（拍照/相册 + 地图选址）
9. 摇一摇设备 → 随机推荐
10. 切换深色/浅色主题
11. 开启大字体模式
