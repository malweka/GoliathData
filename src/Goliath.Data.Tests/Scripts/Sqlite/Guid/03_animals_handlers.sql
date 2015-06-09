Create table animals_handlers(
	AnimalId uniqueidentifier not null,
	EmployeeId uniqueidentifier not null,
	foreign key(AnimalId) references animals(Id),
	foreign key(EmployeeId) references employees(Id),
	primary key(AnimalId, EmployeeId)
);