/* =========================================================================
   Sistema de Gestión de Biblioteca - UV Sucre
   Script de creación de base de datos (equivalente a la migración EF Core)
   Ejecutar en SQL Server Management Studio / Azure Data Studio.
   ========================================================================= */

IF DB_ID('BibliotecaUVSucreDB') IS NULL
BEGIN
    CREATE DATABASE BibliotecaUVSucreDB;
END
GO

USE BibliotecaUVSucreDB;
GO

/* ---------- Tablas de ASP.NET Core Identity ---------- */

IF OBJECT_ID('dbo.AspNetRoles') IS NULL
CREATE TABLE dbo.AspNetRoles (
    Id               NVARCHAR(450)   NOT NULL PRIMARY KEY,
    Name             NVARCHAR(256)   NULL,
    NormalizedName   NVARCHAR(256)   NULL,
    ConcurrencyStamp NVARCHAR(MAX)   NULL
);
GO
CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;
GO

IF OBJECT_ID('dbo.AspNetUsers') IS NULL
CREATE TABLE dbo.AspNetUsers (
    Id                   NVARCHAR(450)   NOT NULL PRIMARY KEY,
    Nombres              NVARCHAR(MAX)   NOT NULL DEFAULT (''),
    Apellidos            NVARCHAR(MAX)   NOT NULL DEFAULT (''),
    UserName             NVARCHAR(256)   NULL,
    NormalizedUserName   NVARCHAR(256)   NULL,
    Email                NVARCHAR(256)   NULL,
    NormalizedEmail      NVARCHAR(256)   NULL,
    EmailConfirmed       BIT             NOT NULL DEFAULT (0),
    PasswordHash         NVARCHAR(MAX)   NULL,
    SecurityStamp        NVARCHAR(MAX)   NULL,
    ConcurrencyStamp     NVARCHAR(MAX)   NULL,
    PhoneNumber          NVARCHAR(MAX)   NULL,
    PhoneNumberConfirmed BIT             NOT NULL DEFAULT (0),
    TwoFactorEnabled     BIT             NOT NULL DEFAULT (0),
    LockoutEnd           DATETIMEOFFSET  NULL,
    LockoutEnabled       BIT             NOT NULL DEFAULT (0),
    AccessFailedCount    INT             NOT NULL DEFAULT (0)
);
GO
CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
CREATE INDEX EmailIndex ON dbo.AspNetUsers(NormalizedEmail);
GO

IF OBJECT_ID('dbo.AspNetRoleClaims') IS NULL
CREATE TABLE dbo.AspNetRoleClaims (
    Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleId     NVARCHAR(450) NOT NULL,
    ClaimType  NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.AspNetUserClaims') IS NULL
CREATE TABLE dbo.AspNetUserClaims (
    Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId     NVARCHAR(450) NOT NULL,
    ClaimType  NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.AspNetUserLogins') IS NULL
CREATE TABLE dbo.AspNetUserLogins (
    LoginProvider       NVARCHAR(450) NOT NULL,
    ProviderKey         NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId              NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.AspNetUserRoles') IS NULL
CREATE TABLE dbo.AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.AspNetUserTokens') IS NULL
CREATE TABLE dbo.AspNetUserTokens (
    UserId        NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(450) NOT NULL,
    Name          NVARCHAR(450) NOT NULL,
    Value         NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

/* ---------- Tablas propias del sistema ---------- */

IF OBJECT_ID('dbo.Libros') IS NULL
CREATE TABLE dbo.Libros (
    Id                  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Titulo              NVARCHAR(200) NOT NULL,
    Autor               NVARCHAR(150) NOT NULL,
    ISBN                NVARCHAR(20)  NOT NULL,
    Editorial           NVARCHAR(100) NOT NULL DEFAULT (''),
    Categoria           NVARCHAR(80)  NOT NULL DEFAULT (''),
    AnioPublicacion     INT           NOT NULL,
    CantidadTotal       INT           NOT NULL,
    CantidadDisponible  INT           NOT NULL
);
GO
CREATE UNIQUE INDEX IX_Libros_ISBN ON dbo.Libros(ISBN);
GO

IF OBJECT_ID('dbo.Prestamos') IS NULL
CREATE TABLE dbo.Prestamos (
    Id                        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LibroId                   INT           NOT NULL,
    UsuarioId                 NVARCHAR(450) NOT NULL,
    FechaSolicitud            DATETIME2     NOT NULL,
    FechaPrestamo             DATETIME2     NULL,
    FechaDevolucionEsperada   DATETIME2     NULL,
    FechaDevolucionReal       DATETIME2     NULL,
    Estado                    INT           NOT NULL,
    CONSTRAINT FK_Prestamos_Libros   FOREIGN KEY (LibroId)   REFERENCES dbo.Libros(Id)      ON DELETE NO ACTION,
    CONSTRAINT FK_Prestamos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.AspNetUsers(Id) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_Prestamos_LibroId   ON dbo.Prestamos(LibroId);
CREATE INDEX IX_Prestamos_UsuarioId ON dbo.Prestamos(UsuarioId);
GO

/* ---------- Datos semilla que NO dependen de hash de contraseña ---------- */
/* Los roles se insertan aquí; los usuarios (con su contraseña bien encriptada)
   los crea automáticamente la aplicación (DbSeeder.cs) la primera vez que se
   ejecuta, así no hay riesgo de generar un PasswordHash inválido a mano.     */

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'ADMINISTRADOR')
INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (NEWID(), 'Administrador', 'ADMINISTRADOR', NEWID());

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'BIBLIOTECARIO')
INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (NEWID(), 'Bibliotecario', 'BIBLIOTECARIO', NEWID());

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'USUARIO')
INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (NEWID(), 'Usuario', 'USUARIO', NEWID());
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Libros)
INSERT INTO dbo.Libros (Titulo, Autor, ISBN, Editorial, Categoria, AnioPublicacion, CantidadTotal, CantidadDisponible)
VALUES
 ('Clean Code', 'Robert C. Martin', '9780132350884', 'Prentice Hall', 'Ingeniería de Software', 2008, 5, 5),
 ('Cien años de soledad', 'Gabriel García Márquez', '9780307474728', 'Sudamericana', 'Literatura', 1967, 3, 3),
 ('Bases de Datos', 'Raghu Ramakrishnan', '9780072465631', 'McGraw-Hill', 'Ciencias de la Computación', 2003, 4, 4);
GO

PRINT 'Base de datos BibliotecaUVSucreDB creada correctamente.';
PRINT 'Ejecuta la aplicación (dotnet run) para que se creen los usuarios admin/bibliotecario/usuario con su contraseña ya encriptada.';
GO
