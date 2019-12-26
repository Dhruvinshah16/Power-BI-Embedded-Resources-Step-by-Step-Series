USE [SamplePBIEmbedded]
GO

/****** Object:  Table [dbo].[Product]    Script Date: 12/26/2019 9:21:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Product](
	[Product] [nvarchar](50) NULL,
	[Profit] [nvarchar](50) NULL,
	[ID] [bigint] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO


