Create table monkeys(
	Id uniqueidentifier not null,
	Family nvarchar(50),
	CanDoTricks bit default(0) not null,
	foreign key(Id) references animals(Id),
	primary key(Id)
);