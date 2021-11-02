using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Execution;

namespace Luax.Interpreter.Test
{
    public static class GenericCollectionAssertionExtension
    {
        public static AndConstraint<GenericCollectionAssertions<T>> HaveElementMatching<T>(this GenericCollectionAssertions<T> collection, Func<T, bool> predicate, string because = null, params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => collection.Subject)
                .ForCondition(enumeration => enumeration.Any(t => predicate(t)))
                .FailWith("Expected collection to contain an element matching the predicate but it does not");
            return new AndConstraint<GenericCollectionAssertions<T>>(collection);
        }

        public static AndConstraint<GenericCollectionAssertions<T>> HaveNotElementsMatching<T>(this GenericCollectionAssertions<T> collection, Func<T, bool> predicate, string because = null, params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => collection.Subject)
                .ForCondition(enumeration => enumeration.All(t => !predicate(t)))
                .FailWith("Expected collection to contain no elements matching the predicate but it does");
            return new AndConstraint<GenericCollectionAssertions<T>>(collection);
        }
    }
}
