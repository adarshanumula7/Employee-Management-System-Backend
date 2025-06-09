IF DB_ID('EmployeeManagementDB') IS NULL
BEGIN
    CREATE DATABASE EmployeeManagementDB;
END
GO

USE EmployeeManagementDB;
GO

-- Create Roles Table
CREATE TABLE [dbo].[roles] (
    [role]     VARCHAR (20) NOT NULL,
    [priority] INT          NULL,
    PRIMARY KEY CLUSTERED ([role] ASC)
);
GO

-- Create Employees Table
CREATE TABLE [dbo].[Employees] (
    [employee_id]       INT             NOT NULL,
    [employee_name]     VARCHAR (100)   NOT NULL,
    [password]          VARCHAR (50)    NULL,
    [gender]            VARCHAR (6)     NOT NULL,
    [dob]               DATE            NOT NULL,
    [phone]             VARCHAR (20)    NULL,
    [email]             VARCHAR (50)    NOT NULL,
    [salary]            DECIMAL (10, 2) NULL,
    [role]              VARCHAR (20)    NULL,
    [job_starting_date] DATE            NULL,
    [isactive]          INT             NULL,
    PRIMARY KEY CLUSTERED ([employee_id] ASC),
    CONSTRAINT [for_role] FOREIGN KEY ([role]) REFERENCES [dbo].[roles] ([role]),
    CONSTRAINT [chk_gender] CHECK ([gender] IN ('Male', 'Female', 'Other'))
);
GO

-- Insert Initial Roles
INSERT INTO [dbo].[roles] ([role], [priority]) 
VALUES 
    ('Admin', 1),
    ('HR', 2),
    ('Employee', 3);
GO

-- Insert Initial Admin User
INSERT INTO [dbo].[Employees] (
    [employee_id],
    [employee_name],
    [password],
    [gender],
    [dob],
    [phone],
    [email],
    [salary],
    [role],
    [job_starting_date],
    [isactive]
) 
VALUES (
    111111,
    'Admin',
    'admin@11',
    'Male',
    '2000-04-04',
    NULL,
    'admin@example.com',
    NULL,
    'Admin',
    NULL,
    0
);
GO

-- Verification Query
SELECT * FROM [dbo].[roles];
SELECT * FROM [dbo].[Employees];