Create table animals_handlers(
	AnimalId int not null,
	EmployeeId int not null,
	foreign key(AnimalId) references animals(Id),
	foreign key(EmployeeId) references employees(Id),
	primary key(AnimalId, EmployeeId)
);