﻿using AnkaCMS.Service.Models;
using AnkaCMS.Core.Globalization;
using FluentValidation;

namespace AnkaCMS.Service.Implementations.ValidationRules.FluentValidation
{
    public class PermissionValidationRules : AbstractValidator<PermissionModel>
    {
        public PermissionValidationRules()
        {
            RuleFor(p => p.Code).NotEmpty().WithMessage(string.Format(Messages.DangerFieldIsEmpty, Dictionary.Code));
            RuleFor(p => p.Name).NotEmpty().WithMessage(string.Format(Messages.DangerFieldIsEmpty, Dictionary.Name));
            RuleFor(p => p.ControllerName).NotEmpty().WithMessage(string.Format(Messages.DangerFieldIsEmpty, Dictionary.ControllerName));
            RuleFor(p => p.ActionName).NotEmpty().WithMessage(string.Format(Messages.DangerFieldIsEmpty, Dictionary.ActionName));
        }
    }
}
