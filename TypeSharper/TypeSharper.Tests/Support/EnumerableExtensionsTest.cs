using System;
using FluentAssertions;
using TypeSharper.Support;
using Xunit;

namespace TypeSharper.Tests.Support;

public class EnumerableExtensionsTest
{
    #region Nested types

    public class ContextWhere
    {
        [Fact]
        public void Handles_out_of_bounds_contexts_by_cropping()
            => new[] { 1, 2, 3 }
               .ContextWhere(100, i => i is 2)
               .Should()
               .BeEquivalentTo(new[] { 1, 2, 3 });

        [Fact]
        public void Includes_contexts_for_all_lines_matching_the_predicate()
            => new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
               .ContextWhere(1, i => i is 2 or 4 or 9)
               .Should()
               .BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 8, 9 });

        [Fact]
        public void Includes_only_contexts_for_filtered_items()
            => new[] { 42, 24 }.ContextWhere(0, i => i == 42).Should().BeEquivalentTo(new[] { 42 });

        [Fact]
        public void Merges_overlapping_contexts_into_one_with_same_prefix_and_suffix_size()
            => new[] { 1, 2, 3, 4, 5, 6, 7, 8 }
               .ContextWhere(1, i => i is 4 or 5)
               .Should()
               .BeEquivalentTo(new[] { 3, 4, 5, 6 });

        [Fact]
        public void Returns_empty_for_empty() => Array.Empty<int>().ContextWhere(0, i => true).Should().BeEmpty();

        [Fact]
        public void Returns_empty_if_nothing_matches()
            => new[] { 1, 2, 3 }.ContextWhere(0, i => false).Should().BeEmpty();
    }

    #endregion
}
