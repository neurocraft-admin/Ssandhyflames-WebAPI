# ?? User & Role List Endpoints - Quick Test Guide

## ? Prerequisites

1. **Database:** Run `SQL/sp_UserRoleList.sql` in SSMS
2. **API:** Build successful (already done ?)
3. **Swagger:** Access at `https://localhost:7183/swagger`

---

## ?? 3-Step Test Sequence

### Step 1: Create Stored Procedures

**Run in SQL Server Management Studio:**
```sql
-- Connect to sandhyaflames database
USE sandhyaflames;
GO

-- Run the entire sp_UserRoleList.sql file
-- It will create:
-- - sp_ListUsers
-- - sp_ListRoles
-- - Run verification queries
```

**Expected Output:**
```
? STORED PROCEDURES CREATED SUCCESSFULLY
```

**Verify Manually:**
```sql
-- Test sp_ListUsers
EXEC sp_ListUsers;

-- Expected: Rows with userId, fullName, email, roleId, roleName, isActive

-- Test sp_ListRoles
EXEC sp_ListRoles;

-- Expected: Rows with roleId, roleName, description, isActive, userCount
```

---

### Step 2: Test in Swagger

**1. Open Swagger UI:**
```
https://localhost:7183/swagger
```

**2. Find "Users" Tag ? "ListUsers" Endpoint:**
```
GET /api/users/list
```

**3. Click "Try it out" ? "Execute"**

**Expected Response (200 OK):**
```json
[
  {
    "userId": 1,
    "fullName": "Admin User",
    "email": "admin@example.com",
    "roleId": 1,
    "roleName": "Administrator",
    "isActive": true
  },
  {
  "userId": 2,
    "fullName": "John Doe",
    "email": "john@example.com",
    "roleId": 2,
    "roleName": "Operator",
    "isActive": true
  }
]
```

**4. Find "Roles" Tag ? "ListRoles" Endpoint:**
```
GET /api/roles/list
```

**5. Click "Try it out" ? "Execute"**

**Expected Response (200 OK):**
```json
[
  {
"roleId": 1,
    "roleName": "Administrator",
    "description": "Full system access",
    "isActive": true,
    "userCount": 1
  },
  {
    "roleId": 2,
    "roleName": "Operator",
    "description": "Limited access",
    "isActive": true,
    "userCount": 2
  }
]
```

---

### Step 3: Test from Browser Console

**1. Open Browser Developer Tools (F12)**

**2. Go to Console Tab**

**3. Test Users List:**
```javascript
fetch('https://localhost:7183/api/users/list')
  .then(res => res.json())
  .then(data => {
    console.log('? Users loaded:', data);
    console.table(data);
  })
  .catch(err => console.error('? Error:', err));
```

**Expected:**
```
? Users loaded: (2) [{...}, {...}]
```

**4. Test Roles List:**
```javascript
fetch('https://localhost:7183/api/roles/list')
  .then(res => res.json())
  .then(data => {
    console.log('? Roles loaded:', data);
    console.table(data);
  })
  .catch(err => console.error('? Error:', err));
```

**Expected:**
```
? Roles loaded: (3) [{...}, {...}, {...}]
```

---

## ?? Troubleshooting

### Issue 1: 404 Not Found

**Symptoms:**
```
GET https://localhost:7183/api/users/list ? 404 Not Found
```

**Cause:** API not running or endpoint not registered

**Solution:**
1. Verify API is running
2. Check Program.cs has `app.MapUserRoutes();`
3. Rebuild and restart API

---

### Issue 2: 400 Bad Request - "Could not find stored procedure"

**Symptoms:**
```json
{
  "success": false,
  "errorCode": "SQL_ERROR",
  "message": "Could not find stored procedure 'sp_ListUsers'"
}
```

**Cause:** Stored procedure doesn't exist

**Solution:**
```sql
-- Run in SSMS
USE sandhyaflames;
GO

-- Check if SP exists
SELECT * FROM sys.procedures WHERE name = 'sp_ListUsers';

-- If not, run sp_UserRoleList.sql
```

---

### Issue 3: Invalid Column Name

**Symptoms:**
```json
{
  "success": false,
  "errorCode": "SQL_ERROR",
  "message": "Invalid column name 'userId'"
}
```

**Cause:** Stored procedure returns wrong column names

**Solution:**
Make sure SP uses exact names (case matters):
- `userId` (not `UserId`)
- `fullName` (not `FullName`)
- `roleName` (not `RoleName`)

**Fix SP:**
```sql
ALTER PROCEDURE sp_ListUsers
AS
BEGIN
    SELECT 
        u.UserId AS userId,  -- ? lowercase 'u'
        u.FullName AS fullName,  -- ? camelCase
        u.Email AS email,
        u.RoleId AS roleId,
        r.RoleName AS roleName,
        u.IsActive AS isActive
  FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId;
END
```

---

### Issue 4: Empty Array Returned

**Symptoms:**
```json
[]
```

**Cause:** No data in tables

**Solution:**
```sql
-- Check if tables have data
SELECT * FROM Users;
SELECT * FROM Roles;

-- If empty, insert sample data (see sp_UserRoleList.sql)
```

---

## ?? Angular Frontend Testing

### Test in Angular App

**1. Navigate to Users Page:**
```
http://localhost:4200/users
```

**Expected:**
- ? Table shows list of users
- ? No 404 error in console
- ? Data loads successfully

**2. Navigate to Roles Page:**
```
http://localhost:4200/roles
```

**Expected:**
- ? Table shows list of roles
- ? No 404 error in console
- ? User count displayed

**3. Check Browser Network Tab:**
```
Network ? Filter: Fetch/XHR
```

**Expected Requests:**
```
? GET /api/users/list ? 200 OK
? GET /api/roles/list ? 200 OK
```

---

## ?? Database Verification

### Check Users Table
```sql
USE sandhyaflames;

-- List all users
SELECT 
    u.UserId,
    u.FullName,
    u.Email,
    r.RoleName,
    u.IsActive
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.RoleId
ORDER BY u.FullName;
```

### Check Roles Table
```sql
-- List all roles with user count
SELECT 
    r.RoleId,
    r.RoleName,
    r.Description,
    r.IsActive,
 COUNT(u.UserId) AS UserCount
FROM Roles r
LEFT JOIN Users u ON r.RoleId = u.RoleId
GROUP BY r.RoleId, r.RoleName, r.Description, r.IsActive
ORDER BY r.RoleName;
```

---

## ? Success Checklist

- [ ] Stored procedures created (`sp_ListUsers`, `sp_ListRoles`)
- [ ] Swagger shows both endpoints under correct tags
- [ ] `/api/users/list` returns 200 OK with user array
- [ ] `/api/roles/list` returns 200 OK with role array
- [ ] Browser console test successful
- [ ] Angular users page loads without errors
- [ ] Angular roles page loads without errors
- [ ] Network tab shows 200 OK responses

---

## ?? Expected Results

### Before Fix:
```
? GET /api/users/list ? 404 Not Found
? GET /api/roles/list ? 404 Not Found
? Angular console: "Failed to load users"
? Angular console: "Failed to load roles"
```

### After Fix:
```
? GET /api/users/list ? 200 OK
? GET /api/roles/list ? 200 OK
? Angular console: "Users loaded: (5) [{...}]"
? Angular console: "Roles loaded: (3) [{...}]"
? Tables populate with data
```

---

## ?? Next Steps

After successful testing:

1. ? **Commit Changes**
```bash
git add .
git commit -m "Add /list endpoints for users and roles"
git push
```

2. ? **Update API Documentation**
   - Endpoints now match Angular frontend expectations
   - No more 404 errors

3. ? **Deploy to Production**
   - Run SQL scripts on production database
   - Deploy API updates

---

**Time to complete:** 5 minutes  
**Expected result:** ? All endpoints working, Angular loads data
