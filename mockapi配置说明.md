# mockapi.io 数据源配置说明

本项目支持从 mockapi.io 读取和新增食物数据。

## 1. 数据来源

- 优先使用 mockapi.io REST API
- 如果没有配置 API 地址，或网络暂时不可用，则使用本地兜底数据，保证 App 不会崩溃

## 2. 在 mockapi.io 创建数据表

1. 打开 [mockapi.io](https://mockapi.io)
2. 新建或进入一个 Project
3. 点击添加 Resource
4. Resource 名称建议填写：

```text
foods
```

5. 添加以下字段：

| 字段名 | 类型建议 | 说明 |
|---|---|---|
| name | String | 食物名称 |
| imageUrl | String | 图片URL |
| region | String | 地区 |
| uploadedBy | String | 上传用户名 |
| description | String | 描述 |
| tags | String | 搜索标签 |

`id` 字段由 mockapi.io 自动生成，不需要手动添加。

## 3. 示例数据

```json
{
  "name": "Peking Duck",
  "imageUrl": "",
  "region": "Beijing, China",
  "uploadedBy": "demo_user",
  "description": "Crispy roast duck served with thin pancakes and sweet bean sauce.",
  "tags": "beijing duck roast chinese"
}
```

## 4. 配置 API 地址

创建 Resource 后，mockapi.io 会生成类似地址：

```text
https://682xxxx.mockapi.io/api/v1/foods
```

打开 `foodApp/Services/MockApiConfig.cs`，把 `EndpointUrl` 改成你的地址：

```csharp
public const string EndpointUrl = "https://682xxxx.mockapi.io/api/v1/foods";
```

## 5. 录屏说明

> 项目使用 mockapi.io REST API 作为数据来源。`FoodService` 通过 HttpClient 获取食物列表、添加新记录和获取详情。如果网络不可用，应用会使用本地兜底数据，避免演示时崩溃。
