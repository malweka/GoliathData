Create table roles(
	Id uniqueidentifier primary key not null,
	Name nvarchar(50) not null,
	Description nvarchar(1000)
);