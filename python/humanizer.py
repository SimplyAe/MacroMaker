"""
Humanization module for adding realistic variance to macro events
"""
import numpy as np
from scipy.interpolate import CubicSpline
from typing import List, Dict, Tuple


class Humanizer:
    """Adds human-like variance to macro events"""
    
    def __init__(self, level: float = 0.5):
        """
        Initialize humanizer
        
        Args:
            level: Humanization level from 0.0 (none) to 1.0 (maximum)
        """
        self.level = max(0.0, min(1.0, level))
        
    def humanize_events(self, events: List[Dict]) -> List[Dict]:
        """
        Apply humanization to a list of macro events
        
        Args:
            events: List of event dictionaries
            
        Returns:
            List of humanized events
        """
        if self.level == 0.0 or not events:
            return events
            
        humanized = []
        
        for event in events:
            humanized_event = event.copy()
            
            # Add timing variance
            if 'Timestamp' in event:
                variance = self._gaussian_noise(0, 5 * self.level)  # Up to 5ms variance
                humanized_event['Timestamp'] = max(0, event['Timestamp'] + variance)
            
            # Add position variance for mouse events
            if event.get('Type') in ['MouseMove', 'MouseLeftDown', 'MouseLeftUp', 
                                      'MouseRightDown', 'MouseRightUp']:
                if 'X' in event and 'Y' in event:
                    x_variance = self._gaussian_noise(0, 2 * self.level)  # Up to 2px variance
                    y_variance = self._gaussian_noise(0, 2 * self.level)
                    humanized_event['X'] = int(event['X'] + x_variance)
                    humanized_event['Y'] = int(event['Y'] + y_variance)
            
            humanized.append(humanized_event)
        
        return humanized
    
    def generate_smooth_path(self, start: Tuple[int, int], end: Tuple[int, int], 
                            num_points: int = 20) -> List[Tuple[int, int]]:
        """
        Generate a smooth Bézier curve path between two points
        
        Args:
            start: Starting (x, y) coordinates
            end: Ending (x, y) coordinates
            num_points: Number of intermediate points
            
        Returns:
            List of (x, y) coordinates forming a smooth path
        """
        # Generate control points with some randomness
        mid_x = (start[0] + end[0]) / 2 + self._gaussian_noise(0, 20 * self.level)
        mid_y = (start[1] + end[1]) / 2 + self._gaussian_noise(0, 20 * self.level)
        
        # Create cubic spline through control points
        t = np.array([0, 0.5, 1.0])
        x = np.array([start[0], mid_x, end[0]])
        y = np.array([start[1], mid_y, end[1]])
        
        cs_x = CubicSpline(t, x)
        cs_y = CubicSpline(t, y)
        
        # Generate smooth path
        t_new = np.linspace(0, 1, num_points)
        path_x = cs_x(t_new)
        path_y = cs_y(t_new)
        
        return [(int(px), int(py)) for px, py in zip(path_x, path_y)]
    
    def add_micro_delays(self, events: List[Dict]) -> List[Dict]:
        """
        Add small random delays between events for more natural timing
        
        Args:
            events: List of event dictionaries
            
        Returns:
            List of events with adjusted timestamps
        """
        if self.level == 0.0 or not events:
            return events
        
        adjusted = []
        cumulative_delay = 0
        
        for event in events:
            adjusted_event = event.copy()
            
            # Add micro delay (0-10ms based on humanization level)
            micro_delay = abs(self._gaussian_noise(0, 10 * self.level))
            cumulative_delay += micro_delay
            
            if 'Timestamp' in event:
                adjusted_event['Timestamp'] = event['Timestamp'] + cumulative_delay
            
            adjusted.append(adjusted_event)
        
        return adjusted
    
    @staticmethod
    def _gaussian_noise(mean: float, std: float) -> float:
        """Generate Gaussian noise"""
        return np.random.normal(mean, std)


def humanize_macro(events: List[Dict], level: float = 0.5) -> List[Dict]:
    """
    Convenience function to humanize a macro
    
    Args:
        events: List of macro events
        level: Humanization level (0.0 to 1.0)
        
    Returns:
        Humanized events
    """
    humanizer = Humanizer(level)
    humanized = humanizer.humanize_events(events)
    humanized = humanizer.add_micro_delays(humanized)
    return humanized
