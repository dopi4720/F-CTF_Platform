import hashlib
import random
import time
from flask import Flask, render_template, request, jsonify
import requests
from CTFd.plugins import bypass_csrf_protection
from CTFd.admin import admin
from CTFd.constants.envvars import API_URL_CONTROLSERVER, PRIVATE_KEY
from CTFd.models import Challenges, Teams
import json

from CTFd.utils.decorators import admin_or_challenge_writer_only_or_jury, admin_or_jury
from CTFd.utils.connector.multiservice_connector import monitoring_control

@admin.route("/admin/monitoring")
@admin_or_challenge_writer_only_or_jury
def monitoring():
    return render_template("admin/monitoring.html")


# Modified API to include more metrics
@admin.route("/api/performance", methods=["POST"])
@admin_or_challenge_writer_only_or_jury
@bypass_csrf_protection
def machine_performance():
    return monitoring_control()