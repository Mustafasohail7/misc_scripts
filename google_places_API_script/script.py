import os
import requests

def download_image(url, folder_path, filename):
    response = requests.get(url, stream=True)
    if response.status_code == 200:
        with open(os.path.join(folder_path, filename), 'wb') as f:
            for chunk in response.iter_content(1024):
                f.write(chunk)
        print(f"Downloaded: {url}")
    else:
        print(f"Failed to download: {url}")

def main():
    folder_name = "downloaded_images"
    if not os.path.exists(folder_name):
        os.makedirs(folder_name)

    base_url = "https://www.abcmix.ro/imaginiconfigurator/"
    to_do = [8650]
    for i in to_do:
        url = f"{base_url}{i}.png"
        filename = f"{i}.png"
        download_image(url, folder_name, filename)

if __name__ == "__main__":
    main()
