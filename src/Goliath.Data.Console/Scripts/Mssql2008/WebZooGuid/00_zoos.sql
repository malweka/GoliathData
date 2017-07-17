Create table zoos(
	Id uniqueidentifier primary key not null,
	Name nvarchar(50) not null,
	City nvarchar(50),
	AcceptNewAnimals bit default(0)  not null
);