USE [SamplePBIEmbedded]
GO

/****** Object:  UserDefinedTableType [dbo].[ProductTableType]    Script Date: 12/26/2019 9:21:03 AM ******/
CREATE TYPE [dbo].[ProductTableType] AS TABLE(
	[Product] [nvarchar](50) NULL,
	[Profit] [nvarchar](50) NULL
)
GO


