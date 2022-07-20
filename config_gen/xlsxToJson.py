from numpy import identity
import pandas as pd
import csv
import json
import os

def export():
    excel_file = './config.xlsx'
    all_sheets = pd.read_excel(excel_file, sheet_name=None)
    sheets = all_sheets.keys()

    for sheet_name in sheets:
        sheet = pd.read_excel(excel_file, sheet_name=sheet_name)
        sheet.to_csv("./%s.csv" % sheet_name, index=False)

def get_json_actor(arr):
    j = {
        "enable": True if arr[1].upper() == 'TRUE' else False,
        "village": arr[2],
        "follower": True if arr[3].upper() == 'TRUE' else False,
        "type": int(arr[4]),
        "nickname": arr[5],
        "pets": [], #arr[6]
        "level": int(arr[7]),
        "prefab": arr[8],
        "position": [], #arr[9]
        "rotation": [], #arr[10]
        "trigger": {
            "type": int(arr[11]),
            "value": arr[12]
        },
        "satisfactions": [], #arr[13]
        "inventory": [], #arr[14]
        "isFly": True if arr[15].upper() == 'TRUE' else False
    }

    #pets
    if len(arr[6]) > 0:
        j['pets'] = arr[6].split(',')
    #position
    j['position'] = [float(p) for p in arr[9].split(',')] 
    #rotation
    j['rotation'] = [float(p) for p in arr[10].split(',')] 
    #satisfaction
    satisfactions = arr[13].split('\n')
    for s in satisfactions:
        arr_s = s.split(',')
        satisfaction = {
            "satisfactionId": arr_s[0],
            "min": int(arr_s[1]),
            "max": int(arr_s[2]),
            "value": int(arr_s[3])
        }
        j['satisfactions'].append(satisfaction)
    
    #inventory
    if len(arr[14]) > 0:
        inventories = arr[14].split('\n')
        for i in inventories:
            arr_i = i.split(',')
            inven = {
                "itemId": arr_i[0],
                "quantity": int(arr_i[1]),
                "installation": True if arr_i[2].upper() == 'TRUE' else False
            }
            j['inventory'].append(inven)

    return j, arr[0]
def actors():
    file = open('./actors.csv')
    csvreader = csv.reader(file)
    header = next(csvreader)

    json_objects = {}
    for row in csvreader:
        j, actor_id = get_json_actor(row)
        json_objects[actor_id] = j
    file.close()

    sz = json.dumps(json_objects, indent=4)
    with open('../config/actors.json', 'w') as f:
        f.write(sz)

def get_json_task(arr):
    j = {
        "id": arr[0],
        "chain": arr[1],
        "type": int(arr[2]),
        "level": [], #arr[3]
        "villageLevel": int(arr[4]),
        "title": arr[5],
        "desc": arr[6],
        "animation": arr[7],
        "animationRepeatTime": int(arr[8]),
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

    #script
    script = []
    if len(arr[17]) > 0:
        script = arr[17].split('\n')
    
    script_refusal = None
    if len(arr[18]) > 0:
        script_refusal = arr[18].split('\n')

    #json, actor type, id, script, script refusal
    return j, int(arr[16]), arr[0], script, script_refusal

def task():
    file = open('./task.csv')
    csvreader = csv.reader(file)
    header = next(csvreader)

    json_task = {}
    json_script = {
        "scripts": {},
        "refusal": {}
    }
    for row in csvreader:
        j, actor_type, script_id, script, script_refusal = get_json_task(row)
        if actor_type in json_task:
            json_task[actor_type].append(j)
        else:
            json_task[actor_type] = [j]

        json_script['scripts'][script_id] = script
        if script_refusal != None:
            json_script['refusal'][script_id] = script_refusal
    file.close()

    sz = json.dumps(json_task, indent=4)
    with open('../config/task.json', 'w') as f:
        f.write(sz)
    
    sz = json.dumps(json_script, indent=4)
    with open('../config/script.json', 'w') as f:
        f.write(sz)

export()
print('exported')
actors()
print('gen actors')
task()
print('gen task')
os.remove('./task.csv')
os.remove('./actors.csv')
print('removed csv files')