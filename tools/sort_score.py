import bs4
import requests
import re
from decimal import Decimal

url = 'http://54.244.193.90:5050/scoreboard/LA'
request = requests.get(url)
soup = bs4.BeautifulSoup(request.text, 'html.parser')
rows = soup.find_all('div', class_='siimple-grid-row')[2].find('div', class_='siimple-grid-col').find('div', class_='siimple-table').find('div', class_='siimple-table-body').find_all('div', class_='siimple-table-row')

rank = {}
for row in rows:
    problem_number = row.find('div', class_='siimple-table-cell').text
    results_raw = row.find('div', class_='siimple-table-cell', text=re.compile('①')).text
    results = re.findall('[\d|\.|\,]+', results_raw, re.MULTILINE)
    rank[int(problem_number)] = {
            '1': Decimal(results[1]),
            '10': Decimal(results[2]),
            }

print('① ')
for key, value in sorted(rank.items(), key = lambda v: v[1]['1']):
    print('#%s: %s%%' % (key, value['1']))

print('⑩')
for key, value in sorted(rank.items(), key = lambda v: v[1]['10']):
    print('#%s: %s%%' % (key, value['10']))
