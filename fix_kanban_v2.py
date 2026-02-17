
import re
import os

file_path = r'DevBoard\Kanban.aspx'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Fix Eval(" Id") -> Eval("Id")
content = content.replace('Eval(" Id")', 'Eval("Id")')

# 2. Fix broken Eval("Type")
# Matches: Eval(" <newline> <spaces> Type")
content = re.sub(r'Eval\(\"\s*[\r\n]+\s*Type\"\)', 'Eval("Type")', content)

# 3. Fix broken Eval("Priority")
# Matches: Eval(" <newline> <spaces> Priority")
content = re.sub(r'Eval\(\"\s*[\r\n]+\s*Priority\"\)', 'Eval("Priority")', content)

# 4. Fix any lingering split attributes (just in case)
# e.g. class="...Eval("...
# code blocks <%# ... %> should be on one line if possible for these simple Evals
# We will use a more aggressive regex if the above doesn't catch it.

# Let's also look for the specific pattern seen in the error:
# class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("
#                                                     Type")) %>"
# We want to turn this into single line.

def replacer(match):
    return match.group(0).replace('\n', '').replace('\r', '').replace('                                                    ', '')

# Attempt to just join the lines for these specific spans if they are still broken
content = re.sub(r'class="badge bg-<%# GetTypeBadgeColor\(\(DevBoard\.Models\.TicketType\)Eval\("\s*[\r\n]+\s*Type"\)\) %>"', 
                 'class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("Type")) %>"', content)

content = re.sub(r'class="badge bg-<%# GetPriorityBadgeColor\(\(DevBoard\.Models\.Priority\)Eval\("\s*[\r\n]+\s*Priority"\)\) %>"', 
                 'class="badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("Priority")) %>"', content)


with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Successfully patched Kanban.aspx v2")
