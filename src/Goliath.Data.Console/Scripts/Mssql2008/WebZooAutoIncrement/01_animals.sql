Create table animals(
	Id int primary key not null identity,
	Name nvarchar(50) not null,
	Age float not null,
	Location char(10),
	ReceivedOn datetime not null,
	ZooId int not null,
	foreign key(ZooId) references zoos(Id)
);