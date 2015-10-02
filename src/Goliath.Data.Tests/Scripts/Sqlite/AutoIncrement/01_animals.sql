Create table animals(
	Id integer primary key autoincrement  not null,
	Name nvarchar(50) not null,
	Age float not null,
	Location char(10),
	ReceivedOn datetime not null,
	ZooId integer not null,
	foreign key(ZooId) references zoos(Id)
);