using Microsoft.EntityFrameworkCore.Storage;
using Mix.Cms.Lib;
using Mix.Cms.Lib.Models.Cms;
using Mix.Cms.Lib.ViewModels.MixAttributeSetDatas;
using Mix.Domain.Core.Models;
using Mix.Domain.Core.ViewModels;
using Mix.Domain.Data.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Mix.Cms.Lib.ViewModels.MixRelatedAttributeDatas
{
	public class UpdateViewModel : ViewModelBase<MixCmsContext, MixRelatedAttributeData, Mix.Cms.Lib.ViewModels.MixRelatedAttributeDatas.UpdateViewModel>
	{
		[JsonProperty("attributeSetId")]
		public int AttributeSetId
		{
			get;
			set;
		}

		[JsonProperty("attributeSetName")]
		public string AttributeSetName
		{
			get;
			set;
		}

		[JsonProperty("createdBy")]
		public string CreatedBy
		{
			get;
			set;
		}

		[JsonProperty("createdDateTime")]
		public DateTime CreatedDateTime
		{
			get;
			set;
		}

		[JsonProperty("cultures")]
		public List<SupportedCulture> Cultures
		{
			get;
			set;
		}

		[JsonProperty("data")]
		public Mix.Cms.Lib.ViewModels.MixAttributeSetDatas.UpdateViewModel Data
		{
			get;
			set;
		}

		[JsonProperty("dataId")]
		public string DataId
		{
			get;
			set;
		}

		[JsonProperty("description")]
		public string Description
		{
			get;
			set;
		}

		[JsonProperty("id")]
		public string Id
		{
			get;
			set;
		}

		[JsonProperty("lastModified")]
		public DateTime? LastModified
		{
			get;
			set;
		}

		[JsonProperty("modifiedBy")]
		public string ModifiedBy
		{
			get;
			set;
		}

		[JsonProperty("parentId")]
		public string ParentId
		{
			get;
			set;
		}

		[JsonProperty("parentName")]
		public string ParentName
		{
			get;
			set;
		}

		[JsonProperty("parentType")]
		public MixEnums.MixAttributeSetDataType ParentType
		{
			get;
			set;
		}

		[JsonProperty("priority")]
		public int Priority
		{
			get;
			set;
		}

		[JsonProperty("specificulture")]
		public string Specificulture
		{
			get;
			set;
		}

		[JsonProperty("status")]
		public MixEnums.MixContentStatus Status
		{
			get;
			set;
		}

		public UpdateViewModel(MixRelatedAttributeData model, MixCmsContext _context = null, IDbContextTransaction _transaction = null)
		{
			base(model, _context, _transaction);
			return;
		}

		public UpdateViewModel()
		{
			base();
			return;
		}

		public override void ExpandView(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
		{
			stackVariable0 = ViewModelBase<MixCmsContext, MixAttributeSetData, Mix.Cms.Lib.ViewModels.MixAttributeSetDatas.UpdateViewModel>.Repository;
			V_1 = Expression.Parameter(Type.GetTypeFromHandle(// 
			// Current member / type: System.Void Mix.Cms.Lib.ViewModels.MixRelatedAttributeDatas.UpdateViewModel::ExpandView(Mix.Cms.Lib.Models.Cms.MixCmsContext,Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
			// Exception in: System.Void ExpandView(Mix.Cms.Lib.Models.Cms.MixCmsContext,Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
			// Specified method is not supported.
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com


		public override MixRelatedAttributeData ParseModel(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
		{
			if (string.IsNullOrEmpty(this.get_Id()))
			{
				this.set_Id(Guid.NewGuid().ToString());
				this.set_CreatedDateTime(DateTime.get_UtcNow());
				if (this.get_Status() == MixEnums.MixContentStatus.Deleted)
				{
					stackVariable18 = Enum.Parse<MixEnums.MixContentStatus>(MixService.GetConfig<string>("DefaultContentStatus"));
				}
				else
				{
					stackVariable18 = this.get_Status();
				}
				this.set_Status(stackVariable18);
			}
			return this.ParseModel(_context, _transaction);
		}
	}
}