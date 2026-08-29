# Comprehensive Assertion Reference

`NinjaTrader.UnitTest` provides a rich assertion library through the [`IAssert`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Assertions/IAssert.cs) interface and the [`Assert`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Assertions/Assert.cs) static utility. Because [`TestCase`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Execution/TestCase.cs) inherits from [`Assert`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Assertions/Assert.cs), all assertion methods can be called directly within any test case.

---

## Complete Assertion Mapping Table

| Python `unittest` Method | C# / NUnit Alias | Description |
| :--- | :--- | :--- |
| **`AssertEqual(a, b)`** | `AreEqual` | Asserts equality via `EqualityComparer<T>.Default`. |
| **`AssertNotEqual(a, b)`** | `AreNotEqual` | Asserts that `a` and `b` are not equal. |
| **`AssertTrue(cond)`** | `IsTrue` | Asserts that boolean `cond` is `true`. |
| **`AssertFalse(cond)`** | `IsFalse` | Asserts that boolean `cond` is `false`. |
| **`AssertIs(a, b)`** | `AreSame` | Asserts that `a` and `b` point to the exact same reference (`ReferenceEquals`). |
| **`AssertIsNot(a, b)`** | `AreNotSame` | Asserts that `a` and `b` do not point to the same reference. |
| **`AssertIsNone(obj)`** | `IsNull` | Asserts that `obj` is `null`. |
| **`AssertIsNotNone(obj)`** | `IsNotNull` | Asserts that `obj` is not `null`. |
| **`AssertIn(item, coll)`** | `Contains` | Asserts that `coll` contains `item`. |
| **`AssertNotIn(item, coll)`** | `DoesNotContain` | Asserts that `coll` does not contain `item`. |
| **`AssertIsInstance<T>(obj)`** | `IsInstanceOfType` | Asserts that `obj` is an instance of `T` (or `Type`). |
| **`AssertNotIsInstance<T>(obj)`** | `IsNotInstanceOfType` | Asserts that `obj` is not an instance of `T` (or `Type`). |
| **`AssertRaises<T>(action)`** | `Throws<T>` | Asserts that invoking `action` throws exception of type `T`. |
| **`AssertRaises(type, action)`** | `Throws(type, action)` | Asserts that invoking `action` throws exception of `Type`. |
| **`AssertRaisesRegex<T>(act, pat)`** | - | Asserts that `act` throws `T` and its message matches regex `pat`. |
| **`AssertAlmostEqual(a, b, places, delta)`** | `AreAlmostEqual` | Asserts floating-point equality within decimal `places` or `delta`. |
| **`AssertNotAlmostEqual(a, b, places, delta)`**| - | Asserts floating-point inequality within decimal `places` or `delta`. |
| **`AssertGreater(a, b)`** | `Greater` | Asserts that `a > b` (`IComparable<T>`). |
| **`AssertGreaterEqual(a, b)`** | `GreaterOrEqual` | Asserts that `a >= b` (`IComparable<T>`). |
| **`AssertLess(a, b)`** | `Less` | Asserts that `a < b` (`IComparable<T>`). |
| **`AssertLessEqual(a, b)`** | `LessOrEqual` | Asserts that `a <= b` (`IComparable<T>`). |
| **`AssertRegex(text, pattern)`** | - | Asserts that string `text` matches regex `pattern`. |
| **`AssertNotRegex(text, pattern)`** | - | Asserts that string `text` does not match regex `pattern`. |
| **`AssertSequenceEqual(seq1, seq2)`** | - | Asserts that two sequences have identical items in the identical order. |
| **`AssertCountEqual(c1, c2)`** | - | Asserts that two collections contain identical elements regardless of order. |
| **`AssertEmpty(coll)`** | `IsEmpty` | Asserts that collection is `null` or contains 0 items. |
| **`AssertNotEmpty(coll)`** | `IsNotEmpty` | Asserts that collection is not `null` and contains 1+ items. |
| **`Fail(message)`** | - | Immediately throws [`AssertionException`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Exceptions/AssertionException.cs). |

---

## Detailed Assertion Categories & Examples

### 1. Equality & Identity

```csharp
// Value equality
AssertEqual(100.0, calculatedPrice);
AssertNotEqual(0.0, currentTick);

// Reference identity (ReferenceEquals)
var instrumentA = MockInstrument.CreateFutures("ES");
var instrumentB = instrumentA;
var instrumentC = MockInstrument.CreateFutures("ES");

AssertIs(instrumentA, instrumentB);
AssertIsNot(instrumentA, instrumentC);

// Null checks
AssertIsNone(order.CancelledTime);
AssertIsNotNone(order.FilledTime);
```

### 2. Booleans & Conditions

```csharp
AssertTrue(order.IsFilled, "Order should be completely filled.");
AssertFalse(position.IsFlat, "Position should not be flat after execution.");
```

### 3. Floating-Point & Precision Comparisons

Financial trading algorithms require robust floating-point comparisons. `AssertAlmostEqual` supports both decimal `places` rounding and absolute `delta` margins:

```csharp
double expectedEma = 5002.37549;
double calculatedEma = 5002.37551;

// Compare using decimal places (default 7 places):
AssertAlmostEqual(expectedEma, calculatedEma, places: 4);

// Compare using absolute delta margin:
AssertAlmostEqual(5000.25, 5000.27, delta: 0.05);

// Inequality check:
AssertNotAlmostEqual(5000.0, 5005.0, delta: 1.0);
```

### 4. Relational & Numeric Comparisons

```csharp
AssertGreater(account.CashValue, 0.0);
AssertGreaterEqual(position.Quantity, 1);
AssertLess(drawdownPercentage, 0.05);
AssertLessEqual(riskPerTrade, 500.0);
```

### 5. Collections & Sequences

```csharp
var openPositions = new List<string> { "ES", "NQ", "YM" };

// Containment
AssertIn("ES", openPositions);
AssertNotIn("RTY", openPositions);

// Sequence equality (checks values and exact ordering)
var list1 = new[] { 10.0, 20.0, 30.0 };
var list2 = new[] { 10.0, 20.0, 30.0 };
AssertSequenceEqual(list1, list2);

// Count equality (checks identical elements regardless of order, like Python's assertCountEqual)
var unordered1 = new[] { "A", "B", "A" };
var unordered2 = new[] { "B", "A", "A" };
AssertCountEqual(unordered1, unordered2);

// Emptiness
AssertEmpty(account.Positions.Values.Where(p => p.IsFlat));
AssertNotEmpty(account.Orders);
```

### 6. Exceptions & Regular Expressions

```csharp
// Assert that a specific exception type is thrown:
ArgumentException ex = AssertRaises<ArgumentException>(() =>
{
    indicator.SetPeriod(-1);
});
AssertEqual("Period must be greater than zero.", ex.Message);

// Assert exception type with Type instance:
AssertRaises(typeof(InvalidOperationException), () =>
{
    account.FillOrder(null, 5000.0, 1);
});

// Assert exception type AND verify message regex:
AssertRaisesRegex<ArgumentOutOfRangeException>(() =>
{
    bars.Close(-5);
}, @"negative index");

// String regex matching:
AssertRegex("Order #1049 filled at 5000.25", @"Order #\d+ filled");
AssertNotRegex("Order #1049 rejected", @"filled");
```

### 7. Custom Failure Messages

All assertions accept an optional `message` string parameter. If provided, the custom message will be displayed upon failure:

```csharp
AssertEqual(expectedTicks, actualTicks, $"Tick count mismatch on bar {currentBar}");
```

---

## Failure Exception Handling

When any assertion fails, it immediately throws [`AssertionException`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Exceptions/AssertionException.cs). 

The test runner captures [`AssertionException`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Exceptions/AssertionException.cs) and records it as a **Failure** in [`TestResult.Failures`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Results/TestResult.cs), keeping it cleanly isolated from unhandled runtime crashes (which are recorded as **Errors** in [`TestResult.Errors`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Results/TestResult.cs)).
