Create table animals_handlers(
	AnimalId integer not null,
	EmployeeId integer not null,
	foreign key(AnimalId) references animals(Id),
	foreign key(EmployeeId) references employees(Id),
	primary key(AnimalId, EmployeeId)
);