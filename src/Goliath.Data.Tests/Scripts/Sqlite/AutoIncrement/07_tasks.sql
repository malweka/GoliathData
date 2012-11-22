Create table tasks(
	Id uniqueidentifier primary key not null,
	title nvarchar(250) null,
	task_description nvarchar(1000),
	created_on datetime not null,
	completed_on datetime,
	assigned_to_id uniqueidentifier,
	foreign key(assigned_to_id) references user_accounts(Id)
);