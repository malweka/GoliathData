Create table employees(
	Id int primary key not null identity,
	FirstName nvarchar(50) not null,
	LastName nvarchar(50) not null,
	EmailAddress nvarchar(50),
	Telephone nvarchar(50),
	Title nvarchar(150),
	HiredOn datetime not null,
	AssignedToZooId int,
	foreign key(AssignedToZooId) references zoos(Id)
);