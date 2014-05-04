Create table user_accounts(
	Id uniqueidentifier primary key not null,
	user_name nvarchar(50) not null,
	email_address nvarchar(50) not null,
	last_access_on datetime,
	account_created_on datetime not null
);