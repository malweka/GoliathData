Create table zoos(
	Id integer primary key autoincrement not null,
	Name nvarchar(50) not null,
	City nvarchar(50),
	AcceptNewAnimals bit default(0)  not null
);