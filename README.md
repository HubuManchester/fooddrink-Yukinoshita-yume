# 食物图鉴 (Food Encyclopedia)

基于 .NET MAUI 的跨平台移动应用，用于浏览、搜索和分享世界各地美食。

## 功能

- **食物图鉴浏览**：查看食物名称、地区、描述和上传者信息
- **搜索**：按名称、地区、描述、标签搜索食物
- **用户系统**：用户名密码注册和登录
- **上传食物**：添加新的食物条目，包含名称、地区、描述和图片URL
- **主题切换**：支持浅色/深色/跟随系统主题
- **大字体模式**：无障碍字体放大

## 技术架构

- **框架**：.NET MAUI (net9.0)
- **数据来源**：支持从 mockapi.io REST API 获取数据，网络不可用时自动使用本地兜底数据
- **本地存储**：用户数据通过 JSON 文件持久化

## 项目结构

```
foodApp/
  Models/
    FoodItem.cs        # 食物数据模型
    User.cs            # 用户模型
  Services/
    MockApiConfig.cs   # mockapi.io 配置
    FoodService.cs     # 食物数据服务（API + 本地兜底）
    UserService.cs     # 用户认证服务
    AccessibilityService.cs  # 无障碍字体缩放
  Pages/
    LoginPage.xaml     # 登录页
    RegisterPage.xaml  # 注册页
    MainPage.xaml      # 主页（食物列表）
    AddFoodPage.xaml   # 添加食物
    FoodDetailPage.xaml # 食物详情
    SettingsPage.xaml  # 设置页
  Platforms/Android/   # Android 平台配置
  Resources/           # 样式、字体、图片资源
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

1. 启动应用，展示登录/注册页面
2. 注册新账户并登录
3. 浏览食物图鉴列表，使用搜索功能
4. 查看食物详情
5. 添加新的食物条目
6. 切换深色/浅色主题
7. 开启大字体模式
