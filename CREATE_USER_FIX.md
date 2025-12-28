# ? CreateUserAsync Fix - Read SP Response Correctly

## ?? **Issue Found:**

The `CreateUserAsync` method was returning `false` even when user creation succeeded because:
- Used `ExecuteNonQueryAsync()` which returns affected rows
- Stored procedure `sp_CreateUser` returns a SELECT statement (result set)
- `ExecuteNonQueryAsync()` returns -1 when a result set is returned
- The check `rows > 0` was always false (-1 is not > 0)

---

## ? **Solution:**

Changed `CreateUserAsync` to use `ExecuteReaderAsync()` to read the `success` column from the stored procedure result set.

---

## ?? **Method Fixed:**

| Method | Before | After | Status |
|--------|--------|-------|--------|
| `CreateUserAsync` | ? ExecuteNonQueryAsync | ? ExecuteReaderAsync | FIXED |

**Note:** `UpdateUserAsync` and `SoftDeleteUserAsync` were NOT modified yet - they need testing first to confirm if their stored procedures return SELECT statements or just do UPDATE/DELETE.

---

## ?? **Before (Incorrect):**

```csharp
public static async Task<bool> CreateUserAsync(string connStr, CreateUserRequest user)
{
    using var conn = new SqlConnection(connStr);
    using var cmd = new SqlCommand("sp_CreateUser", conn)
    {
      CommandType = CommandType.StoredProcedure
    };

    cmd.Parameters.AddWithValue("@FullName", user.FullName);
    cmd.Parameters.AddWithValue("@Email", user.Email);
    cmd.Parameters.AddWithValue("@PasswordHash", PasswordHelper.ComputeSha256Hash(user.Password));
    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);

    await conn.OpenAsync();
    var rows = await cmd.ExecuteNonQueryAsync();  // ? Returns -1 when SP has SELECT
    return rows > 0;  // ? Always false because -1 is not > 0
}
```

**Problem:**
- `ExecuteNonQueryAsync()` returns -1 when stored procedure has SELECT statement
- Check `rows > 0` evaluates to `false` (-1 > 0 = false)
- Frontend receives "User creation failed" even though user was created

---

## ? **After (Correct):**

```csharp
public static async Task<bool> CreateUserAsync(string connStr, CreateUserRequest user)
{
    using var conn = new SqlConnection(connStr);
    using var cmd = new SqlCommand("sp_CreateUser", conn)
    {
CommandType = CommandType.StoredProcedure
    };

    cmd.Parameters.AddWithValue("@FullName", user.FullName);
    cmd.Parameters.AddWithValue("@Email", user.Email);
    cmd.Parameters.AddWithValue("@PasswordHash", PasswordHelper.ComputeSha256Hash(user.Password));
    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);

  await conn.OpenAsync();
    using var reader = await cmd.ExecuteReaderAsync();  // ? Read result set
    
    if (await reader.ReadAsync())
    {
        var success = reader.GetInt32(reader.GetOrdinal("success"));  // ? Get success column
        return success == 1;  // ? Return true if success = 1
    }
    
    return false;  // ? Return false if no rows returned
}
```

**Solution:**
- Uses `ExecuteReaderAsync()` to read the result set
- Reads the `success` column from the stored procedure response
- Returns `true` if `success = 1`, `false` otherwise

---
