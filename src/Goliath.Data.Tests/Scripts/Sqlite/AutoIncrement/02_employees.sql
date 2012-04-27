Create table employees(
	Id integer primary key autoincrement  not null,
	FirstName nvarchar(50) not null,
	LastName nvarchar(50) not null,
	EmailAddress nvarchar(50),
	Telephone nvarchar(50),
	Title nvarchar(150),
	HiredOn datetime not null,
	AssignedToZooId integer,
	foreign key(AssignedToZooId) references zoos(Id)
);