from asyncore import write
import csv
import json

def get_json(arr):
    j = {
        "id": arr[0],
        "chain": arr[1],
        "type": int(arr[2]),
        "level": [], #arr[3]
        "villageLevel": int(arr[4]),
        "title": arr[5],
        "desc": arr[6],
        "animation": arr[7],
        "time": int(arr[8]),
        "maxRef": int(arr[9]),
        "target": {
            "type": int(arr[10]),
            "value": [], #arr[11]                
            "interaction": {
                "type": int(arr[12]),
                "taskId": arr[13]
            }      
        },
        "satisfactions": {}, #arr[14]
        "satisfactionsRefusal": {} #arr[15]
    }
    #level
    if len(arr[3]) > 0: 
        level = arr[3].split(',')
        j['level'].append(int(level[0]))
        j['level'].append(int(level[1]))

    #target.value
    j['target']['value'] = arr[11].split('\n')
    #satisfactions
    if len(arr[14]) > 0 :
        satisfactions = arr[14].split(',')
        for s in satisfactions:
            kv = s.split(':')
            j['satisfactions'][kv[0]] = int(kv[1])

    #satisfactionsRefusal
    if(len(arr[15]) > 0):
        satisfactionsRefusal = arr[15].split(',')
        for s in satisfactionsRefusal:
            kv = s.split(':')
            j['satisfactionsRefusal'][kv[0]] = int(kv[1])

    return j, int(arr[16]) #json, actor type

file = open('./task.csv')
csvreader = csv.reader(file)
header = next(csvreader)

json_objects = {}
for row in csvreader:
    j, actor_type = get_json(row)
    if actor_type in json_objects:
        json_objects[actor_type].append(j)
    else:
        json_objects[actor_type] = [j]
file.close()

sz = json.dumps(json_objects, indent=4)
with open('task.json', 'w') as f:
    f.write(sz)

print(sz)