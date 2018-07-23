import bs4
import requests
import re
import sys
from decimal import Decimal

url = 'http://54.244.193.90:5050/scoreboard/%s' % (sys.argv[1])
request = requests.get(url)
soup = bs4.BeautifulSoup(request.text, 'html.parser')
rows = soup.find_all('div', class_='siimple-grid-row')[2].find('div', class_='siimple-grid-col').find('div', class_='siimple-table').find('div', class_='siimple-table-body').find_all('div', class_='siimple-table-row')

rank = {}
for row in rows:
    problem_number = row.find('div', class_='siimple-table-cell').text
    results_raw = row.find('div', class_='siimple-table-cell', text=re.compile('①'))
    if results_raw is None:
        results = [Decimal('0.0'), Decimal('0.0')]
    else:
        results_raw = results_raw.text
        results = re.findall('[\d|\.|\,]+', results_raw, re.MULTILINE)
        results = list(map(lambda d: Decimal(d), results[1:]))
    rank[int(problem_number)] = {
            '1': results[0],
            '10': results[1],
            }

print('① ')
for key, value in sorted(rank.items(), key = lambda v: v[1]['1']):
    print('#%s: %s%%' % (key, value['1']))

print('⑩')
for key, value in sorted(rank.items(), key = lambda v: v[1]['10']):
    print('#%s: %s%%' % (key, value['10']))
