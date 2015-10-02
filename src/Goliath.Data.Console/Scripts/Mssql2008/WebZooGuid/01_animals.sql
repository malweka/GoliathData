Create table animals(
	Id uniqueidentifier primary key not null,
	Name nvarchar(50) not null,
	Age float not null,
	Location char(10),
	ReceivedOn datetime not null,
	ZooId uniqueidentifier not null,
	foreign key(ZooId) references zoos(Id)
);