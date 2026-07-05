namespace Lyn.Backend.Apps.Calorie.Enums;

/// <summary>
/// Hvor et FoodProduct i felleskatalogen kommer fra. External er forberedt
/// for fremtidig import (f.eks. Kassal.app / Open Food Facts) — ingen
/// integrasjon finnes ennå.
/// </summary>
public enum ProductSource
{
    User = 0,
    Admin = 1,
    External = 2
}
