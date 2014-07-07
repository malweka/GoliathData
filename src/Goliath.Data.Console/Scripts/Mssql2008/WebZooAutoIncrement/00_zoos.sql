Create table zoos(
	Id int primary key not null identity,
	Name nvarchar(50) not null,
	City nvarchar(50),
	AcceptNewAnimals bit default(0)  not null
);