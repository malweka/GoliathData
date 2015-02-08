#Metadata Attributes

Attributes can be added to entity definition for carrying more meta data about entities or entity properties. In XMl, the meta data attribute must be prefixed
with a *data_* prefix.

##Data validation Attributes

These attributes are useful for data validation.

| Attribute | Type | Description |
|-----------|------|-------------|
| required | bool | For required fields, should be true or false |
| editable | bool | Whether a field is editable or not, should be true or false |
| min | int | minimum value |
| max | int | maximum value |


##Visual Display Attributes

These attributes are only useful during view or control generations

| Attribute | Type | Description |
|-----------|------|-------------|
| display_description| string | entity or property description |
| display_order | int | A numerical value to set the display order |
| display_groupname | string | For fields that need to be grouped together for display purpose |
| display_propmpt | string | prompt |
| display_prefctrl | string | prefered control type. Valid values should be *checkbox*, *radio*, *text*, *readonly*, *numeric*, *editor*, *dropdown*, *date*, *time*, *textarea*, *label*, *color* |