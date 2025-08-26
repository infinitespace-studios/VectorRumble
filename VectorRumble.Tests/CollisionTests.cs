using Microsoft.Xna.Framework;
using VectorRumble;

namespace VectorRumble.Tests;

public class CollisionTests
{
    [Fact]
    public void CircleCircleIntersect_TouchingCircles_ReturnsTrue()
    {
        // Arrange - Two circles touching exactly at one point
        Vector2 center1 = new Vector2(0, 0);
        float radius1 = 5f;
        Vector2 center2 = new Vector2(10, 0);
        float radius2 = 5f;
        
        // Act
        bool result = Collision.CircleCircleIntersect(center1, radius1, center2, radius2);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void CircleCircleIntersect_OverlappingCircles_ReturnsTrue()
    {
        // Arrange - Two overlapping circles
        Vector2 center1 = new Vector2(0, 0);
        float radius1 = 5f;
        Vector2 center2 = new Vector2(5, 0);
        float radius2 = 5f;
        
        // Act
        bool result = Collision.CircleCircleIntersect(center1, radius1, center2, radius2);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void CircleCircleIntersect_SeparateCircles_ReturnsFalse()
    {
        // Arrange - Two separate circles with gap between them
        Vector2 center1 = new Vector2(0, 0);
        float radius1 = 3f;
        Vector2 center2 = new Vector2(10, 0);
        float radius2 = 3f;
        
        // Act
        bool result = Collision.CircleCircleIntersect(center1, radius1, center2, radius2);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void CircleCircleIntersect_OneCircleInsideAnother_ReturnsTrue()
    {
        // Arrange - Small circle inside large circle
        Vector2 center1 = new Vector2(0, 0);
        float radius1 = 10f;
        Vector2 center2 = new Vector2(2, 2);
        float radius2 = 3f;
        
        // Act
        bool result = Collision.CircleCircleIntersect(center1, radius1, center2, radius2);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void CircleCircleIntersect_IdenticalCircles_ReturnsTrue()
    {
        // Arrange - Two identical circles at same position
        Vector2 center1 = new Vector2(5, 5);
        float radius1 = 7f;
        Vector2 center2 = new Vector2(5, 5);
        float radius2 = 7f;
        
        // Act
        bool result = Collision.CircleCircleIntersect(center1, radius1, center2, radius2);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void LineLineIntersect_IntersectingLines_ReturnsTrue()
    {
        // Arrange - Two lines that cross
        Vector2 a = new Vector2(0, 0);
        Vector2 b = new Vector2(10, 10);
        Vector2 c = new Vector2(0, 10);
        Vector2 d = new Vector2(10, 0);
        
        // Act
        bool result = Collision.LineLineIntersect(a, b, c, d, out Vector2 intersection);
        
        // Assert
        Assert.True(result);
        Assert.Equal(5f, intersection.X, 0.001f);
        Assert.Equal(5f, intersection.Y, 0.001f);
    }
    
    [Fact]
    public void LineLineIntersect_ParallelLines_ReturnsFalse()
    {
        // Arrange - Two parallel lines that never intersect
        Vector2 a = new Vector2(0, 0);
        Vector2 b = new Vector2(10, 0);
        Vector2 c = new Vector2(0, 5);
        Vector2 d = new Vector2(10, 5);
        
        // Act
        bool result = Collision.LineLineIntersect(a, b, c, d, out Vector2 intersection);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void LineLineIntersect_NonIntersectingLines_ReturnsFalse()
    {
        // Arrange - Two line segments that would intersect if extended, but don't intersect as segments
        Vector2 a = new Vector2(0, 0);
        Vector2 b = new Vector2(2, 2);
        Vector2 c = new Vector2(3, 0);
        Vector2 d = new Vector2(5, 2);
        
        // Act
        bool result = Collision.LineLineIntersect(a, b, c, d, out Vector2 intersection);
        
        // Assert
        Assert.False(result);
    }
}