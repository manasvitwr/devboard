
import re
import os

file_path = r'DevBoard\Kanban.aspx'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: Eval(" Id") -> Eval("Id")
content = content.replace('Eval(" Id")', 'Eval("Id")')

# Fix 2: Broken string literals in Eval
# Pattern: Eval(" followed by optional whitespace/newline, then the value, then ")
# We specifically target the cases where the string is broken across lines
# The string start is on one line: Eval("
# The string end is on next line: <spaces>Type")

# We can just look for the specific broken patterns observed
# Type
content = re.sub(r'Eval\(\"\s*[\r\n]+\s*Type\"\)', 'Eval("Type")', content)
# Priority
content = re.sub(r'Eval\(\"\s*[\r\n]+\s*Priority\"\)', 'Eval("Priority")', content)

# Also fix the weird breaking of the tag itself if necessary, but the error is CS1010 (Newline in constant)
# so fixing the string literal should start the tag parsing correctly.

# Let's also clean up the tag split if it looks ugly, but the mainly the Eval is the issue.
# The previous view_file showed:
# class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("
#                                                     Type")) %>"
# This becomes:
# class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("Type")) %>"
# Which is valid.

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Successfully patched Kanban.aspx")
