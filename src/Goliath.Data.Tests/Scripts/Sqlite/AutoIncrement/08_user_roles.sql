Create table user_roles(
	user_id uniqueidentifier not null,
	role_id uniqueidentifier not null,
	foreign key(user_id) references user_accounts(Id),
	foreign key(role_id) references roles(Id),
	primary key(user_id, role_id)
);