using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace InfoSecurity
{
    public static class EntityMetadata
    {
        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            ["ID"] = "ID", ["Login"] = "Логин", ["Role"] = "Роль", ["FIO"] = "ФИО",
            ["Photo"] = "Фотография", ["IsActive"] = "Активен", ["Name"] = "Название",
            ["Description"] = "Описание", ["Position"] = "Должность", ["Email"] = "Эл. почта",
            ["Phone"] = "Телефон", ["DepartmentID"] = "ID подразделения", ["UserID"] = "ID пользователя",
            ["AssetType"] = "Тип актива", ["InventoryNumber"] = "Инвентарный номер",
            ["Criticality"] = "Критичность (1–5)", ["Title"] = "Заголовок",
            ["IncidentDate"] = "Дата инцидента", ["Severity"] = "Серьёзность (1–5)",
            ["Status"] = "Статус", ["AssetID"] = "ID актива", ["AssignedUserID"] = "ID ответственного",
            ["Threat"] = "Угроза", ["Vulnerability"] = "Уязвимость",
            ["Probability"] = "Вероятность (1–5)", ["Impact"] = "Влияние (1–5)",
            ["AssessmentDate"] = "Дата оценки", ["Mitigation"] = "Меры реагирования",
            ["RiskScore"] = "Оценка риска", ["Version"] = "Версия",
            ["ApprovalDate"] = "Дата утверждения", ["Content"] = "Содержание",
            ["EventDate"] = "Дата события", ["Action"] = "Действие",
            ["Details"] = "Сведения", ["IpAddress"] = "IP-адрес"
        };

        public static string DisplayName(PropertyInfo property)
        {
            return Names.TryGetValue(property.Name, out string name) ? name : property.Name;
        }

        public static List<PropertyInfo> GetScalarProperties(Type entityType, bool forEditing = false)
        {
            return entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != "PasswordHash")
                .Where(p => IsSupported(p.PropertyType))
                .Where(p => !forEditing || (p.CanWrite && p.Name != "ID"))
                .OrderBy(p => p.Name == "ID" ? 0 : 1)
                .ToList();
        }

        public static bool IsSupported(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(string) || type == typeof(int) || type == typeof(bool) ||
                   type == typeof(DateTime) || type == typeof(decimal) || type == typeof(double);
        }

        public static string GetFilterProperty(Type entityType)
        {
            if (entityType == typeof(User)) return nameof(User.Role);
            if (entityType == typeof(SecurityIncident)) return nameof(SecurityIncident.Status);
            if (entityType == typeof(SecurityAsset)) return nameof(SecurityAsset.AssetType);
            if (entityType == typeof(Employee)) return nameof(Employee.Position);
            return null;
        }
    }
}
