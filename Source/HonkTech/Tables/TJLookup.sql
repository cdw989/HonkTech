﻿SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[TJLookup](
	[TJLookupID] [int] IDENTITY(1,1) NOT NULL,
	[TJID] [int] NOT NULL,
	[RecordType] [int] NOT NULL,
	[RecordID] [int] NOT NULL,
 CONSTRAINT [pk_SSTRecordTJMap_pid] PRIMARY KEY CLUSTERED 
(
	[TJLookupID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]