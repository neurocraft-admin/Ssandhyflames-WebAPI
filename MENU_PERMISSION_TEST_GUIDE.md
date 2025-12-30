# ?? Menu & Permissions - Quick Test Guide

## ? Prerequisites

1. **JWT Token:** Obtain valid token from `/api/auth/login`
2. **Database:** Stored procedures and tables exist
3. **Swagger:** Access at `https://localhost:7183/swagger`

---

## ?? 5-Step Test Sequence

### Step 1: Get Current User Menu

**Request:**
```
GET /api/menu/current-user
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response (200 OK):**
```json
[
  {
  "name": "Dashboard",
    "url": "/dashboard",
    "iconComponent": { "name": "cilSpeedometer" },
    "children": null
  },
  {
    "name": "Admin",
"url": "/admin",
    "iconComponent": { "name": "cilSettings" },
    "children": [
      { "name": "Users", "url": "/admin/users", "iconComponent": { "name": "cilPeople" } },
      { "name": "Roles", "url": "/admin/roles", "iconComponent": { "name": "cilShieldAlt" } }
    ]
  }
]
```

**Verify:**
- ? Top-level items returned
- ? Children nested correctly
- ? IconComponent format matches CoreUI
- ? Null children for items without sub-menus

---

### Step 2: Get Current User Permissions

**Request:**
```
GET /api/permissions/current-user
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response (200 OK):**
```json
{
  "userId": 1,
  "username": "admin",
  "roleId": 1,
  "roleName": "Administrator",
  "permissions": [
    {
    "resourceId": 1,
      "resourceName": "Dashboard",
      "canView": true,
      "canCreate": false,
      "canUpdate": false,
      "canDelete": false
    },
    {
      "resourceId": 2,
   "resourceName": "Users",
      "canView": true,
      "canCreate": true,
      "canUpdate": true,
      "canDelete": true
    }
  ]
}
```

**Verify:**
- ? User info returned
- ? Role info included
- ? Permissions array populated
- ? All CRUD flags present

---

### Step 3: Check Single Permission

**Request:**
```
GET /api/permissions/check?resource=Users&action=Create
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response (200 OK):**
```json
{
  "allowed": true
}
```

**Test Different Actions:**
```
GET /api/permissions/check?resource=Users&action=View
GET /api/permissions/check?resource=Users&action=Update
GET /api/permissions/check?resource=Users&action=Delete
GET /api/permissions/check?resource=Products&action=Create
```

**Verify:**
- ? Returns true for allowed permissions
- ? Returns false for denied permissions
- ? 400 Bad Request if resource/action missing

---

### Step 4: Update Role Permissions

**Request:**
```
PUT /api/permissions/role/2
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "resourceId": 5,
  "canView": true,
  "canCreate": true,
  "canUpdate": false,
  "canDelete": false
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Permissions updated successfully"
}
```

**Verify:**
- ? Permissions updated in database
- ? Success message returned
- ? Check with GET /api/permissions/current-user to see changes

---

### Step 5: Get Roles Summary

**Request:**
```
GET /api/roles/permissions
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response (200 OK):**
```json
[
  {
    "roleId": 1,
    "roleName": "Administrator",
    "totalResources": 16,
    "viewCount": 16,
    "createCount": 16,
    "updateCount": 16,
    "deleteCount": 16
  },
  {
    "roleId": 2,
    "roleName": "Operator",
  "totalResources": 8,
    "viewCount": 8,
    "createCount": 4,
    "updateCount": 4,
    "deleteCount": 0
  }
]
```

**Verify:**
- ? All roles returned
- ? Permission counts accurate
- ? TotalResources matches database

---

## ?? Error Testing

### Test 1: Missing JWT Token
```
GET /api/menu/current-user
(No Authorization header)
```
**Expected:** 401 Unauthorized

### Test 2: Invalid JWT Token
```
GET /api/menu/current-user
Authorization: Bearer invalid-token
```
**Expected:** 401 Unauthorized

### Test 3: Missing Query Parameters
```
GET /api/permissions/check
(No resource or action)
```
**Expected:** 400 Bad Request
```json
{
  "success": false,
  "message": "Resource and action are required"
}
```

### Test 4: Invalid User ID in Token
```
GET /api/menu/current-user
Authorization: Bearer <token-with-userId="abc">
```
**Expected:** 400 Bad Request
```json
{
  "success": false,
  "errorCode": "INVALID_USER_ID",
  "message": "Invalid user ID in token"
}
```

---

## ?? Database Verification

### Check Menu Items
```sql
SELECT * FROM MenuItems ORDER BY SortPath;
```

### Check Permissions
```sql
SELECT 
    r.RoleName,
    res.ResourceName,
    p.CanView,
    p.CanCreate,
    p.CanUpdate,
    p.CanDelete
FROM Permissions p
INNER JOIN Roles r ON p.RoleId = r.RoleId
INNER JOIN Resources res ON p.ResourceId = res.ResourceId
ORDER BY r.RoleName, res.ResourceName;
```

### Check Menu Access
```sql
SELECT 
    r.RoleName,
    m.Title,
    m.Url
FROM MenuAccess ma
INNER JOIN Roles r ON ma.RoleId = r.RoleId
INNER JOIN MenuItems m ON ma.MenuItemId = m.MenuItemId
ORDER BY r.RoleName, m.DisplayOrder;
```

---

## ?? Swagger UI Testing

### Access Swagger:
```
https://localhost:7183/swagger
```

### Find Endpoints:
Look for **"Menu & Permissions"** tag

### Authorize:
1. Click "Authorize" button
2. Enter: `Bearer <your-jwt-token>`
3. Click "Authorize"
4. Click "Close"

### Test Each Endpoint:
1. Expand endpoint
2. Click "Try it out"
3. Enter parameters (if needed)
4. Click "Execute"
5. Check response

---

## ?? Troubleshooting

### Issue: 401 Unauthorized
**Cause:** No JWT token or expired token
**Solution:** Login again to get fresh token

### Issue: Menu returns empty array
**Cause:** User's role has no menu access
**Solution:** Check MenuAccess table, add entries

### Issue: Permissions returns empty array
**Cause:** User's role has no permissions
**Solution:** Check Permissions table, add entries

### Issue: Check permission returns false
**Cause:** Resource/Action not granted to user's role
**Solution:** Update permissions using endpoint 4

---

## ? Success Criteria

- [x] All 5 endpoints return 200 OK
- [x] Menu structure is hierarchical
- [x] Permissions include CRUD flags
- [x] Permission check works correctly
- [x] Update permissions succeeds
- [x] Roles summary shows counts

---

## ?? Next: Angular Integration

After testing in Swagger:

1. Create MenuService in Angular
2. Call `/api/menu/current-user` on app init
3. Bind to CoreUI sidebar
4. Create PermissionGuard for routes
5. Use permission check in components

---

**Time to complete:** 10 minutes  
**Expected result:** ? All endpoints working
