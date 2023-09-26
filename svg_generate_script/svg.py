import os
import svgwrite


# Define the characters to generate SVGs for
characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'

# Define the gradient colors
gradient_colors = ['#514D4D', '#444']
color = 'black'
colors = ['#c23291', '#da2629', '#d1913a', '#e46c4a', '#73c259', '#309867', '#2da3a7', '#2d3b7c', '#454648']
color_names = ['pink','red','yellow','orange','lgreen','green','lblue','blue','black']
start_colors = ['#d36b7e', '#f24d4e', '#e0a96c', '#f19278', '#9ad68f', '#4ab18d', '#4aa1b4', '#4a5e89', '#606060']
stop_colors = ['#d45964', '#dd3f47', '#e1b95e', '#eb8d6b', '#8bce7a', '#3ea476', '#49b1b5', '#405294', '#4d5451']

# Create a directory to store the SVGs if needed
# Ensure the 'output' directory exists

if not os.path.exists('output'):
    os.makedirs('output')

# Loop through each character
index=0
for c in colors:
    for char in characters:
        # Create a new SVG document
        dwg = svgwrite.Drawing(f'output/{char}_{color_names[index]}.svg', profile='tiny', size=('100%', '100%'))

        # Define the linear gradient
        gradient = svgwrite.gradients.LinearGradient(start=(0, 0), end=('100%', 0))
        gradient.add_stop_color(offset='0%', color=start_colors[index])
        gradient.add_stop_color(offset='100%', color=stop_colors[index])
        dwg.defs.add(gradient)

        # Add the gradient line
        gradient_line = dwg.line(start=('38%', '85.1%'), end=('62%', '85%'), stroke=gradient.get_paint_server(), stroke_width=20)
        dwg.add(gradient_line)

        # Add the text element
        text_element = dwg.text(char, insert=('50%', '82.5%'), font_family='Arial', font_size=160,
                                text_anchor='middle', fill=c, font_weight='bold')
        dwg.add(text_element)

        # Add the black line
        black_line = dwg.line(start=('38%', '93%'), end=('62%', '93%'), stroke=c, stroke_width=20)
        dwg.add(black_line)

        # Save the SVG file
        dwg.save()
    index+=1

print("SVGs generated successfully.")
