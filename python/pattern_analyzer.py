"""
Pattern analyzer for detecting repetitive patterns in macros
"""
from typing import List, Dict
from collections import Counter


class PatternAnalyzer:
    """Analyzes macros for patterns and optimization opportunities"""
    
    def analyze_macro(self, events: List[Dict]) -> Dict:
        """
        Analyze a macro for patterns and statistics
        
        Args:
            events: List of macro events
            
        Returns:
            Dictionary containing analysis results
        """
        if not events:
            return {
                'total_events': 0,
                'event_types': {},
                'patterns': [],
                'suggestions': []
            }
        
        # Count event types
        event_types = Counter(event.get('Type', 'Unknown') for event in events)
        
        # Detect repetitive sequences
        patterns = self._detect_patterns(events)
        
        # Generate optimization suggestions
        suggestions = self._generate_suggestions(events, event_types, patterns)
        
        return {
            'total_events': len(events),
            'event_types': dict(event_types),
            'patterns': patterns,
            'suggestions': suggestions,
            'duration_ms': events[-1].get('Timestamp', 0) if events else 0
        }
    
    def _detect_patterns(self, events: List[Dict], min_length: int = 3) -> List[Dict]:
        """
        Detect repetitive patterns in event sequences
        
        Args:
            events: List of events
            min_length: Minimum pattern length to detect
            
        Returns:
            List of detected patterns
        """
        patterns = []
        
        # Simple pattern detection: look for repeated event type sequences
        event_types = [e.get('Type', '') for e in events]
        
        for length in range(min_length, len(event_types) // 2):
            for i in range(len(event_types) - length * 2):
                pattern = event_types[i:i + length]
                next_sequence = event_types[i + length:i + length * 2]
                
                if pattern == next_sequence:
                    patterns.append({
                        'pattern': pattern,
                        'length': length,
                        'start_index': i,
                        'repetitions': 2  # Could be extended to count more
                    })
        
        return patterns[:5]  # Return top 5 patterns
    
    def _generate_suggestions(self, events: List[Dict], event_types: Counter, 
                             patterns: List[Dict]) -> List[str]:
        """Generate optimization suggestions"""
        suggestions = []
        
        # Check for excessive mouse moves
        if event_types.get('MouseMove', 0) > len(events) * 0.7:
            suggestions.append("High number of mouse movements. Consider reducing recording sensitivity.")
        
        # Check for patterns
        if patterns:
            suggestions.append(f"Detected {len(patterns)} repetitive patterns. Consider using loops.")
        
        # Check for very short delays
        timestamps = [e.get('Timestamp', 0) for e in events]
        if len(timestamps) > 1:
            delays = [timestamps[i+1] - timestamps[i] for i in range(len(timestamps)-1)]
            avg_delay = sum(delays) / len(delays) if delays else 0
            
            if avg_delay < 10:
                suggestions.append("Very fast macro. Consider adding delays for reliability.")
        
        return suggestions


def analyze_macro_patterns(events: List[Dict]) -> Dict:
    """
    Convenience function to analyze macro patterns
    
    Args:
        events: List of macro events
        
    Returns:
        Analysis results
    """
    analyzer = PatternAnalyzer()
    return analyzer.analyze_macro(events)
