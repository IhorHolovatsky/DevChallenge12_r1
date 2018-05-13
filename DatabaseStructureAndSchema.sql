/****** Object:  Table [dbo].[CDN_ServerRole]    Script Date: 12.05.2018 00:03:13 ******/

/****** Object:  Table [dbo].[CDN_ServerRole]    Script Date: 12.05.2018 00:03:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CDN_ServerRole](
	[Id] [int] NOT NULL,
	[Name] [nchar](100) NOT NULL,
	[Description] [nchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO





/****** Object:  Table [dbo].[CDN_Server]    Script Date: 12.05.2018 00:03:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CDN_Server](
	[Id] [int] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[FreeSpace] [bigint] NOT NULL,
	[IpAddress] [nchar](50) NOT NULL,
	[Host] [nchar](50) NOT NULL,
	[IsOnline] [bit] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[ServerRoleId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CDN_Server]  WITH CHECK ADD FOREIGN KEY([ServerRoleId])
REFERENCES [dbo].[CDN_ServerRole] ([Id])
GO


/****** Object:  Table [dbo].[CDN_FileObject]    Script Date: 12.05.2018 00:03:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CDN_FileObject](
	[Id] [nvarchar](400) NOT NULL,
	[VersionId] [int] NOT NULL,
	[ServerId] [int] NOT NULL,
	[Size] [bigint] NOT NULL,
	[DateUploaded] [datetime] NULL,
	[LastAccess] [datetime] NULL,
	[UploadId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[ServerId] ASC,
	[VersionId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CDN_FileObject]  WITH CHECK ADD FOREIGN KEY([ServerId])
REFERENCES [dbo].[CDN_Server] ([Id])
GO


INSERT INTO CDN_ServerRole(Id, Name, Description)
VALUES (1, 'Origin Server', null),
(2, 'Cache Server', null),
(3, 'Route Server', null)