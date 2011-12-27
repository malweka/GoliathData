if exists(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'animals_handlers') AND type in (N'U'))
drop table animals_handlers;

if exists(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'monkeys') AND type in (N'U'))
drop table monkeys;

if exists(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'employees') AND type in (N'U'))
drop table employees;

if exists(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'animals') AND type in (N'U'))
drop table animals;

if exists(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'zoos') AND type in (N'U'))
drop table zoos;