#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

# Dockerfile that creates a container suitable to build dotnet-cli
FROM microsoft/dotnet-buildtools-prereqs:alpine-3.6-3148f11-20171119021156

RUN apk update && apk upgrade && apk add --no-cache curl ncurses

# This Dockerfile doesn't use the USER_ID, but the parameter needs to be declared to prevent docker
# from issuing a warning
ARG USER_ID=0
RUN adduser code_executor -u ${USER_ID} -G root -D
RUN echo 'code_executor ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers

# With the User Change, we need to change permssions on these directories
RUN chmod -R a+rwx /usr/local
RUN chmod -R a+rwx /home

# Set working directory 
ARG WORK_DIR
WORKDIR ${WORK_DIR}

# Set up Azure Artifacts credential provider
# We download the installer and execute it using ${USER_ID} so that the installer
# put the NuGet plugins in the correct $HOME/.nuget folder.
RUN curl -O https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh
RUN chmod +x ./installcredprovider.sh

# Set user to the one we just created
USER ${USER_ID}

RUN ./installcredprovider.sh
