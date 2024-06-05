using System.Collections;
using RightVisionBot.Common;
using System.Text;
using Org.BouncyCastle.Bcpg;

namespace RightVisionBot.Types
{
    public class UserPermissions : IEnumerable<Permission>
    {
        private readonly long? _userId;
        public List<Permission> Collection { get; set; }
        public List<Permission> Removed { get; set; } = new();


        public UserPermissions(long? userId = null)
        {
            Collection = new();
            if (userId == null) return;
            _userId = userId;
            Program.database.Read($"UPDATE RV_Users SET permissions = '{this}' WHERE userId = {_userId};", "");
        }

        public UserPermissions(List<Permission> permissions, long? userId = null)
        {
            Collection = permissions;
            if (userId == null) return;
            _userId = userId;
            Program.database.Read($"UPDATE RV_Users SET permissions = '{this}' WHERE userId = {_userId};", "");
        }

        public UserPermissions(UserPermissions permissions, long? userId = null)
        {
            Collection = permissions.Collection;
            if (userId != null)
            {
                _userId = userId;
                Program.database.Read($"UPDATE RV_Users SET permissions = '{this}' WHERE userId = {_userId};", "");
            }
        }

        public UserPermissions(long? userId = null, params Permission[] permissions)
        {
            Collection = new List<Permission>(permissions);
            if (userId == null) return;
            _userId = userId;
            Program.database.Read($"UPDATE RV_Users SET permissions = '{this}' WHERE userId = {_userId};", "");
        }

        public IEnumerator<Permission> GetEnumerator() => Collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var perm in Collection)
                sb.Append(perm + ";");
            foreach (var blockedPerm in Removed)
                if(!string.IsNullOrEmpty(blockedPerm.ToString()))
                    sb.Append("::" + blockedPerm + ";");

            return sb.ToString();
        }

        public static UserPermissions operator +(UserPermissions left, UserPermissions right)
        {
            var combined = new UserPermissions(left.Collection);
            combined.AddList(right);
            return combined;
        }

        public static UserPermissions operator +(UserPermissions left, Permission right)
        {
            UserPermissions combined = new(left) { right };
            return combined;
        }

        public static UserPermissions operator -(UserPermissions left, UserPermissions right)
        {
            UserPermissions combined = new(left.Collection);
            combined.RemoveList(right);
            return combined;
        }

        public static UserPermissions operator -(UserPermissions left, Permission right)
        {
            UserPermissions combined = new(left.Collection);
            combined.Remove(right);
            return combined;
        }

        public int Count => Collection.Count;

        //Данный метод создан для того, чтобы записывать в базу данных сразу группу прав, а не отправлять по запросу на каждое право
        private void AddList(IEnumerable<Permission> list)
        {
            List<Permission> combinedList = new List<Permission>(Collection);
            foreach (var permission in list)
                if (!combinedList.Contains(permission))
                    combinedList.Add(permission);

            if (_userId != null)
                Program.database.Read($"UPDATE RV_Users SET permissions = '{this}' WHERE userId = {_userId};", "");
            Collection = combinedList;
        }

        //Данный метод создан для того, чтобы записывать в базу данных сразу группу прав, а не отправлять по запросу на каждое право
        private void RemoveList(IEnumerable<Permission> list)
        {
            List<Permission> combinedList = new List<Permission>(list);
            foreach (var permission in combinedList.Where(permission => !combinedList.Contains(permission)))
                combinedList.Remove(permission);

            if (_userId != null)
                Program.database.Read($"UPDATE RV_Users SET permissions = '{this}' WHERE userId = {_userId};", "");
            Collection = combinedList;
        }

        public void Add(Permission permission)
        {
            if (Collection.Contains(permission)) return;
            Collection.Add(permission);
            Removed.Remove(permission);
            if (_userId != null)
                Program.database.Read($"UPDATE RV_Users SET permissions = '{this}' WHERE userId = {_userId};", "");
        }

        public void Remove(Permission permission)
        {
            Collection.Remove(permission);
            Removed.Add(permission);
            if (_userId != null)
                Program.database.Read($"UPDATE RV_Users SET permissions = '{this}' WHERE userId = {_userId};", "");
        }

        public bool Contains(Permission permission) => Collection.Contains(permission);
    }
}
