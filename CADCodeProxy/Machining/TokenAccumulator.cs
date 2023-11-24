using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

internal class TokenAccumulator {

    private List<IMachiningOperation> _operations = new();
    private List<IRouteSequenceSegment> _currentSequence = new();
    private Fillet? _currentFillet = null;

    public void AddToken(IToken token) {

        if (token is Fillet fillet) {

            var lastToken = _currentSequence.LastOrDefault();
            if (lastToken is not Route && lastToken is not OutlineSegment) {
                throw new InvalidOperationException("Fillets must exist between routes or outline segments");
            }

            _currentFillet = fillet;
            return;

        } else if (token is Rectangle rectangle) {

            foreach (var subTokens in rectangle.GetComponentTokens()) {
                AddToken(subTokens);
            }

            return;

        } else if (_currentFillet is not null) {

            if (token is Route route) {

                var lastToken = _currentSequence.LastOrDefault();
                if (lastToken is not Route lastRoute) {
                    throw new InvalidOperationException("Fillets must exist between two entities of the same type");
                }

                if (route.Start != lastRoute.End ||
                    route.SequenceNumber != lastRoute.SequenceNumber ||
                    route.NumberOfPasses != lastRoute.NumberOfPasses ||
                    route.ToolName != lastRoute.ToolName ||
                    route.StartDepth != lastRoute.EndDepth ||
                    route.Offset != lastRoute.Offset) {
                    throw new InvalidOperationException("Fillets must exist between two entities which are connected");
                }

                var result = FilletRoutes(lastRoute, route, _currentFillet);
                _currentFillet = null;

                _currentSequence.Remove(lastToken);
                _currentSequence.Add(result.Route1);
                _currentSequence.Add(result.Arc);
                _currentSequence.Add(result.Route2);

            } else if (token is OutlineSegment outlineSegment) {

                var lastToken = _operations.LastOrDefault();
                if (_currentSequence.Any() || lastToken is not OutlineSegment lastSegment) {
                    throw new InvalidOperationException("Fillets must exist between two entities of the same type");
                }

                if (outlineSegment.Start != lastSegment.End) {
                    throw new InvalidOperationException("Fillets must exist between two entities which are connected");
                }

                var result = FilletOutline(lastSegment, outlineSegment, _currentFillet);
                _currentFillet = null;

                _operations.Remove(lastToken);
                _operations.Add(result.Segment1);
                _operations.Add(result.Segment2);
                _operations.Add(result.Segment3);

            } else {
                throw new InvalidOperationException("Fillets must exist between routes or outline segments");
            }

            return;

        } else if (token is IRouteSequenceSegment segment) {

            var lastToken = _currentSequence.LastOrDefault();
            if (lastToken is null) {

                _currentSequence.Add(segment);

            } else if (lastToken.ToolName == segment.ToolName && lastToken.End == segment.Start) {

                _currentSequence.Add(segment);

            } else {
            
                AddCurrentSequence();
                _currentSequence.Add(segment);

            }

            return;

        } else if (token is IMachiningOperation operation) {

            AddCurrentSequence();

            _operations.Add(operation);

        } else {
            throw new InvalidOperationException($"Unexpected token {token.GetType().Name}");
        }

    }

    private void AddCurrentSequence() {

        if (!_currentSequence.Any()) {
            return;
        }

        var first = _currentSequence.First();
        var last = _currentSequence.Last();

        if (first.Start == last.End) {

            _operations.Add(new SetMill() {
                ToolName = first.ToolName,
                Start = first.Start,
                StartDepth = first.StartDepth,
                Offset = first.Offset,
                SequenceNumber = first.SequenceNumber,
                NumberOfPasses = first.NumberOfPasses,
                FeedSpeed = first.FeedSpeed,
                SpindleSpeed = first.SpindleSpeed
            });

        }

        foreach (var segment in _currentSequence) {
            _operations.Add(segment);
        }

        _currentSequence = new();

    }

    private static (Route Route1, Arc Arc, Route Route2) FilletRoutes(Route a, Route b, Fillet fillet) {

        var points = FilletCalculator.GetFilletPoints(new(a.Start.X, a.Start.Y),
                                        new(a.End.X, a.End.Y),
                                        new(b.End.X, b.End.Y),
                                        fillet.Radius);

        a.End = points.Item1;
        b.Start = points.Item2;

        var arc = new Arc() {
            Start = a.End,
            End = b.Start,
            Radius = fillet.Radius,
            Direction = points.CounterClockWise ? ArcDirection.CounterClockWise : ArcDirection.ClockWise,

            ToolName = a.ToolName,
            StartDepth = a.StartDepth,
            EndDepth = a.EndDepth,
            Offset = a.Offset,
            SequenceNumber = a.SequenceNumber,
            NumberOfPasses = a.NumberOfPasses,
            FeedSpeed = a.FeedSpeed,
            SpindleSpeed = a.SpindleSpeed
        };

        return (a, arc, b);

    }

    private (OutlineSegment Segment1, ArcOutlineSegment Segment2, OutlineSegment Segment3) FilletOutline(OutlineSegment a, OutlineSegment b, Fillet fillet) {

        var points = FilletCalculator.GetFilletPoints(a.Start,
                                        a.End,
                                        b.End,
                                        fillet.Radius);

        a.End = points.Item1;
        b.Start = points.Item2;

        var arc = new ArcOutlineSegment() {
            Start = a.End,
            End = b.Start,
            Radius = fillet.Radius,
            Direction = points.CounterClockWise ? ArcDirection.CounterClockWise : ArcDirection.ClockWise,

            ToolName = a.ToolName,
            StartDepth = a.StartDepth,
            EndDepth = a.EndDepth,
            SequenceNumber = a.SequenceNumber,
            NumberOfPasses = a.NumberOfPasses,
            FeedSpeed = a.FeedSpeed,
            SpindleSpeed = a.SpindleSpeed
        };

        return (a, arc, b);

    }

    public IMachiningOperation[] GetMachiningOperations() {

        // If this method is called before all tokens in a sequence are added, the subsequent sequences will not be added to the current sequence.
        // This class could be designed to avoid that, although it is probably not necessary
        
        AddCurrentSequence();

        return _operations.ToArray();

    }

    internal class FilletCalculator {

        internal static (Point, Point, bool CounterClockWise) GetFilletPoints(Point start, Point center, Point end, double radius) {

            var line1 = new Line(start, center);
            var line2 = new Line(center, end);

            bool counterClockWise = false;
            double xMult, yMult;
            if (IsPointToLeftOfLine(line1, end)) {
                counterClockWise = true;
                // Left
                xMult = 1;
                yMult = -1;
            } else {
                // Right
                xMult = -1;
                yMult = 1;
            }

            var uVec1 = GetUnitVectorInDirectionOfLine(line1);          // A unitary vector in the direction of line 1
            var perpVec1 = new Vector2(uVec1.X * xMult, uVec1.Y * yMult) * radius;   // A vector perpendicular to line 1, scaled by the value of radius
            var perpPoint1 = GetPointOffsetInDirectionOfVector(line1.A, perpVec1); // The point which is offset from the starting point of the start of line 1, perpendicular to line 1

            var uVec2 = GetUnitVectorInDirectionOfLine(line2);
            var perpVec2 = new Vector2(uVec2.X * xMult, uVec2.Y * yMult) * radius;
            var perpPoint2 = GetPointOffsetInDirectionOfVector(line2.A, perpVec2);

            // Calculate intersection of parallel lines, represented by the perpendicular points and unit vectors
            // Find the tangent points to the circle centered at point center with a radius radius
            double denominator = (uVec1.X * uVec2.Y) - (uVec2.X * uVec1.Y);
            var tangent1 = GetTangentPoints(start, uVec1, uVec2, perpPoint1, perpPoint2, denominator);
            var tangent2 = GetTangentPoints(center, uVec2, uVec1, perpPoint1, perpPoint2, denominator);

            if (double.IsNaN(tangent1.X) || double.IsInfinity(tangent1.X)
                || double.IsNaN(tangent1.Y) || double.IsInfinity(tangent1.Y)
                || double.IsNaN(tangent2.X) || double.IsInfinity(tangent2.X)
                || double.IsNaN(tangent2.Y) || double.IsInfinity(tangent2.Y)) {

                throw new InvalidOperationException("Invalid fillet operation between points");

            }

            // TODO: make sure these points are returned in the correct order;
            return (tangent1, tangent2, counterClockWise);

        }

        private static Point GetTangentPoints(Point start, Vector2 unitVector1, Vector2 unitVector2, Point perpPoint1, Point perpPoint2, double denominator) {

            // Calculates a point which is tangent to a circle on a line which is parallel to a line going through point 'start' in the direction of unitVector1

            double k1 = (unitVector2.Y * (perpPoint2.X - perpPoint1.X) - unitVector2.X * (perpPoint2.Y - perpPoint1.Y)) / denominator;

            return new Point(
                start.X + k1 * unitVector1.X,
                start.Y + k1 * unitVector1.Y);

        }

        private static Vector2 GetUnitVectorInDirectionOfLine(Line line) {

            var le = Math.Sqrt(Math.Pow(line.B.X - line.A.X, 2.0) + Math.Pow(line.B.Y - line.A.Y, 2));

            var vx = (line.B.X - line.A.X) / le;
            var vy = (line.B.Y - line.A.Y) / le;

            return new Vector2(vx, vy);

        }

        private static Point GetPointOffsetInDirectionOfVector(Point point, Vector2 offset) {

            double x1 = point.X + offset.Y;
            double y1 = point.Y + offset.X;

            return new(x1, y1);

        }

        private static bool IsPointToLeftOfLine(Line line, Point point) {

            return (line.B.X - line.A.X) * (point.Y - line.A.Y) - (line.B.Y - line.A.Y) * (point.X - line.A.X) > 0;

        }

        internal record Line(Point A, Point B);
        internal record Vector2(double X, double Y) {
            public static Vector2 operator *(Vector2 left, double right) => new Vector2(left.X * right, left.Y * right);
        }

    }

}
