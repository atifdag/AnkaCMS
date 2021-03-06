﻿using AnkaCMS.Data.DataAccess.EntityFramework;
using AnkaCMS.Data.DataEntities;
using AnkaCMS.Service.Models;
using AnkaCMS.Core;
using AnkaCMS.Core.CrudBaseModels;
using AnkaCMS.Core.Exceptions;
using AnkaCMS.Core.Globalization;
using AnkaCMS.Core.Helpers;
using AnkaCMS.Core.Security;
using AnkaCMS.Core.Validation.FluentValidation;
using AnkaCMS.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using AnkaCMS.Service.Implementations.ValidationRules.FluentValidation;

namespace AnkaCMS.Service.Implementations
{
    public class ParameterGroupService : IParameterGroupService
    {

        private readonly IRepository<ParameterGroup> _repositoryParameterGroup;
        private readonly IRepository<ParameterGroupHistory> _repositoryParameterGroupHistory;
        private readonly IRepository<User> _repositoryUser;
        private readonly IRepository<Parameter> _repositoryParameter;
        private readonly IMainService _serviceMain;

        public ParameterGroupService(IRepository<ParameterGroup> repositoryParameterGroup, IRepository<ParameterGroupHistory> repositoryParameterGroupHistory, IRepository<User> repositoryUser, IRepository<Parameter> repositoryParameter, IMainService serviceMain)
        {
            _repositoryParameterGroup = repositoryParameterGroup;
            _repositoryParameterGroupHistory = repositoryParameterGroupHistory;
            _repositoryUser = repositoryUser;

            _repositoryParameter = repositoryParameter;
            _serviceMain = serviceMain;
        }


        private User IdentityUser
        {
            get
            {
                // Thread'de kayıtlı kimlik bilgisi alınıyor
                var identity = (AnkaCMSIdentity)Thread.CurrentPrincipal.Identity;

                User user;

                // Veritabanından sorgulanıyor
                user = _repositoryUser.Get()
                    .Join(x => x.Person)
                    .FirstOrDefault(a => a.Id == identity.UserId);




                // Kullanıcı bulunamadı ise
                if (user == null)
                {
                    throw new NotFoundException(Messages.DangerIdentityUserNotFound);
                }

                return user;
            }
        }


        public ListModel<ParameterGroupModel> List(FilterModel filterModel)
        {
            var startDate = filterModel.StartDate.ResetTimeToStartOfDay();
            var endDate = filterModel.EndDate.ResetTimeToEndOfDay();
            Expression<Func<ParameterGroup, bool>> expression;

            if (filterModel.Status != -1)
            {
                var status = filterModel.Status.ToString().ToBoolean();
                if (filterModel.Searched != null)
                {
                    if (status)
                    {
                        expression = c => c.IsApproved && c.Name.Contains(filterModel.Searched);
                    }
                    else
                    {
                        expression = c => c.IsApproved == false && c.Name.Contains(filterModel.Searched);
                    }
                }
                else
                {
                    if (status)
                    {
                        expression = c => c.IsApproved;
                    }
                    else
                    {
                        expression = c => c.IsApproved == false;
                    }
                }

            }
            else
            {
                if (filterModel.Searched != null)
                {
                    expression = c => c.Name.Contains(filterModel.Searched);
                }
                else
                {
                    expression = c => c.Id != Guid.Empty;
                }
            }

            expression = expression.And(e => e.CreationTime >= startDate && e.CreationTime <= endDate);

            var model = filterModel.CreateMapped<FilterModel, ListModel<ParameterGroupModel>>();

            if (model.Paging == null)
            {
                model.Paging = new Paging
                {
                    PageSize = filterModel.PageSize,
                    PageNumber = filterModel.PageNumber
                };
            }

            var sortHelper = new SortHelper<ParameterGroupModel>();

            var query = _repositoryParameterGroup
                .Join(x => x.Creator.Person)
                .Join(x => x.LastModifier.Person)

                .Where(expression);

            sortHelper.OrderBy(x => x.DisplayOrder);

            model.Paging.TotalItemCount = query.Count();
            var items = model.Paging.PageSize > 0 ? query.Skip((model.Paging.PageNumber - 1) * model.Paging.PageSize).Take(model.Paging.PageSize) : query;
            var modelItems = new HashSet<ParameterGroupModel>();
            foreach (var item in items)
            {
                var modelItem = item.CreateMapped<ParameterGroup, ParameterGroupModel>();
                modelItem.Creator = new IdCodeName(item.Creator.Id, item.Creator.Username, item.Creator.Person.DisplayName);
                modelItem.LastModifier = new IdCodeName(item.LastModifier.Id,item.LastModifier.Username, item.LastModifier.Person.DisplayName);
                modelItems.Add(modelItem);
            }
            model.Items = modelItems.ToList();
            var pageSizeDescription = _serviceMain.ApplicationSettings.PageSizeList;
            var pageSizes = pageSizeDescription.Split(',').Select(s => new KeyValuePair<int, string>(s.ToInt(), s)).ToList();
            pageSizes.Insert(0, new KeyValuePair<int, string>(-1, "[" + Dictionary.All + "]"));
            model.Paging.PageSizes = pageSizes;
            model.Paging.PageCount = (int)Math.Ceiling((float)model.Paging.TotalItemCount / model.Paging.PageSize);
            if (model.Paging.TotalItemCount > model.Items.Count)
            {
                model.Paging.HasNextPage = true;
            }

            // ilk sayfa ise

            if (model.Paging.PageNumber == 1)
            {
                if (model.Paging.TotalItemCount > 0)
                {
                    model.Paging.IsFirstPage = true;
                }

                // tek sayfa ise

                if (model.Paging.PageCount == 1)
                {
                    model.Paging.IsLastPage = true;
                }

            }

            // son sayfa ise
            else if (model.Paging.PageNumber == model.Paging.PageCount)
            {
                model.Paging.HasNextPage = false;
                // tek sayfa değilse

                if (model.Paging.PageCount > 1)
                {
                    model.Paging.IsLastPage = true;
                    model.Paging.HasPreviousPage = true;
                }
            }

            // ara sayfa ise
            else
            {
                model.Paging.HasNextPage = true;
                model.Paging.HasPreviousPage = true;
            }

            if (model.Paging.TotalItemCount > model.Items.Count && model.Items.Count <= 0)
            {
                model.Message = Messages.DangerRecordNotFoundInPage;
            }

            if (model.Paging.TotalItemCount == 0)
            {
                model.Message = Messages.DangerRecordNotFound;
            }
            return model;
        }

        public DetailModel<ParameterGroupModel> Detail(Guid id)
        {
            var item = _repositoryParameterGroup.Join(x => x.Creator.Person)
                .Join(x => x.LastModifier.Person).FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                throw new NotFoundException(Messages.DangerRecordNotFound);
            }
            var modelItem = item.CreateMapped<ParameterGroup, ParameterGroupModel>();
            modelItem.Creator = new IdCodeName(item.Creator.Id, item.Creator.Username, item.Creator.Person.DisplayName);
            modelItem.LastModifier = new IdCodeName(item.LastModifier.Id,item.LastModifier.Username, item.LastModifier.Person.DisplayName);
            return new DetailModel<ParameterGroupModel>
            {
                Item = modelItem
            };
        }

        public AddModel<ParameterGroupModel> Add()
        {
            return new AddModel<ParameterGroupModel>();
        }

        public AddModel<ParameterGroupModel> Add(AddModel<ParameterGroupModel> addModel)
        {
            IValidator validator = new FluentValidator<ParameterGroupModel, ParameterGroupValidationRules>(addModel.Item);
            var validationResults = validator.Validate();
            if (!validator.IsValid)
            {
                throw new ValidationException(Messages.DangerInvalidEntitiy)
                {
                    ValidationResult = validationResults
                };
            }

            var item = addModel.Item.CreateMapped<ParameterGroupModel, ParameterGroup>();

            if (_repositoryParameterGroup.Get().FirstOrDefault(e => e.Code == item.Code) != null)
            {
                throw new DuplicateException(string.Format(Messages.DangerFieldDuplicated, Dictionary.Code));
            }
            item.Id = GuidHelper.NewGuid();
            item.Version = 1;
            item.CreationTime = DateTime.Now;
            item.LastModificationTime = DateTime.Now;
            item.DisplayOrder = 1;

            item.Creator = IdentityUser ?? throw new IdentityUserException(Messages.DangerIdentityUserNotFound);
            item.LastModifier = IdentityUser;
            _repositoryParameterGroup.Add(item, true);
            var maxDisplayOrder = _repositoryParameterGroup.Get().Max(e => e.DisplayOrder);
            item.DisplayOrder = maxDisplayOrder + 1;
            var affectedItem = _repositoryParameterGroup.Update(item, true);
            var itemHistory = affectedItem.CreateMapped<ParameterGroup, ParameterGroupHistory>();
            itemHistory.Id = GuidHelper.NewGuid();
            itemHistory.ReferenceId = affectedItem.Id;
            itemHistory.CreatorId = IdentityUser.Id;
            _repositoryParameterGroupHistory.Add(itemHistory, true);

            addModel.Item = affectedItem.CreateMapped<ParameterGroup, ParameterGroupModel>();

            addModel.Item.Creator = new IdCodeName(IdentityUser.Id,IdentityUser.Username, IdentityUser.Person.DisplayName);
            addModel.Item.LastModifier = new IdCodeName(IdentityUser.Id,IdentityUser.Username, IdentityUser.Person.DisplayName);

            return addModel;
        }

        public UpdateModel<ParameterGroupModel> Update(Guid id)
        {
            var item = _repositoryParameterGroup.Join(x => x.Creator.Person)
                .Join(x => x.LastModifier.Person).FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                throw new NotFoundException(Messages.DangerRecordNotFound);
            }
            var modelItem = item.CreateMapped<ParameterGroup, ParameterGroupModel>();
            modelItem.Creator = new IdCodeName(item.Creator.Id, item.Creator.Username, item.Creator.Person.DisplayName);
            modelItem.LastModifier = new IdCodeName(item.LastModifier.Id,item.LastModifier.Username, item.LastModifier.Person.DisplayName);
            return new UpdateModel<ParameterGroupModel>
            {
                Item = modelItem
            };
        }

        public UpdateModel<ParameterGroupModel> Update(UpdateModel<ParameterGroupModel> updateModel)
        {
            IValidator validator = new FluentValidator<ParameterGroupModel, ParameterGroupValidationRules>(updateModel.Item);
            var validationResults = validator.Validate();
            if (!validator.IsValid)
            {
                throw new ValidationException(Messages.DangerInvalidEntitiy)
                {
                    ValidationResult = validationResults
                };
            }

            var item = _repositoryParameterGroup.Join(x => x.Creator.Person)
                .Join(x => x.LastModifier.Person).FirstOrDefault(e => e.Id == updateModel.Item.Id);
            if (item == null)
            {
                throw new NotFoundException(Messages.DangerRecordNotFound);
            }



            if (updateModel.Item.Code != item.Code)
            {
                if (_repositoryParameterGroup.Get().Any(p => p.Code == updateModel.Item.Code))
                {
                    throw new DuplicateException(string.Format(Messages.DangerFieldDuplicated, Dictionary.Code));
                }
            }

            var versionHistoryParameterGroup = _repositoryParameterGroupHistory.Get().Where(e => e.ReferenceId == item.Id).Max(t => t.Version);

            var itemHistory = item.CreateMapped<ParameterGroup, ParameterGroupHistory>();
            itemHistory.Id = GuidHelper.NewGuid();
            itemHistory.ReferenceId = item.Id;
            itemHistory.CreatorId = IdentityUser.Id;
            itemHistory.IsDeleted = false;
            itemHistory.Version = versionHistoryParameterGroup + 1;
            itemHistory.RestoreVersion = 0;
            _repositoryParameterGroupHistory.Add(itemHistory, true);

            item.Code = updateModel.Item.Code;
            item.Name = updateModel.Item.Name;
            item.Description = updateModel.Item.Description;
            item.IsApproved = updateModel.Item.IsApproved;
            item.LastModificationTime = DateTime.Now;
            item.LastModifier = IdentityUser;

            item.LastModificationTime = DateTime.Now;
            item.LastModifier = IdentityUser;
            var version = item.Version;
            item.Version = version + 1;
            var affectedItem = _repositoryParameterGroup.Update(item, true);

            updateModel.Item = affectedItem.CreateMapped<ParameterGroup, ParameterGroupModel>();


            updateModel.Item.Creator = new IdCodeName(item.Creator.Id, item.Creator.Username, item.Creator.Person.DisplayName);
            updateModel.Item.LastModifier = new IdCodeName(IdentityUser.Id,IdentityUser.Username, IdentityUser.Person.DisplayName);

            return updateModel;

        }

        public void Delete(Guid id)
        {
            if (_repositoryParameter.Get().Count(x => x.ParameterGroup.Id == id) > 0)
            {
                throw new InvalidTransactionException(Messages.DangerAssociatedRecordNotDeleted);
            }

            var version = _repositoryParameterGroupHistory.Get().Where(e => e.ReferenceId == id).Max(t => t.Version);
            var item = _repositoryParameterGroup.Get(x => x.Id == id);
            if (item == null)
            {
                throw new NotFoundException(Messages.DangerRecordNotFound);
            }

            var itemHistory = item.CreateMapped<ParameterGroup, ParameterGroupHistory>();

            itemHistory.Id = GuidHelper.NewGuid();
            itemHistory.ReferenceId = item.Id;
            itemHistory.CreationTime = DateTime.Now;

            itemHistory.CreatorId = IdentityUser.Id;
            itemHistory.Version = version + 1;
            itemHistory.IsDeleted = true;

            _repositoryParameterGroupHistory.Add(itemHistory, true);
            _repositoryParameterGroup.Delete(item, true);
        }

        public List<IdCodeName> List()
        {
            var list = _repositoryParameterGroup.Get().Where(x => x.IsApproved).OrderBy(x => x.DisplayOrder).Select(x => new IdCodeName(x.Id, x.Code, x.Name));
            if (list.Any())
            {
                return list.ToList();
            }
            throw new NotFoundException(Messages.DangerRecordNotFound);
        }
    }
}