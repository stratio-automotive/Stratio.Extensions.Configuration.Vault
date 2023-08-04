"""
Utility file that contains multiple helper methods.
"""

@staticmethod
def color_text(text, color):
    """
    Wrap the text with ANSI escape codes to apply color.

    Args:
        text (str): The text to be colored.
        color (str): The color code. e.g., 'green', 'yellow', 'red'.

    Returns:
        str: The colored text.
    """
    colors = {
        'green': '\033[92m',   # Green
        'yellow': '\033[93m',  # Yellow
        'red': '\033[91m'      # Red
    }
    end_color = '\033[0m'     # Reset color
    return f"{colors[color]}{text}{end_color}"
